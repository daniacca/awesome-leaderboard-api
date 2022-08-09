using MongoDB.Driver;
using NeoSmart.Hashing.XXHash.Core;
using NoSql.MongoDb.Abstraction.Interfaces;
using System.Text;
using System.Threading.Tasks;

namespace Common.NoSql.AbstractClasses
{
    public abstract class AbsNoSqlAction<TIn, TOut, TContext> : INoSqlHashValidator<TContext>, INoSqlAction<TOut,TIn>, INoSqlActionAsync<TOut, TIn> where TContext : class, INoSqlCollection, new()
    {
        protected INoSqlSessionProvider Session { get; }

        protected virtual IMongoCollection<TContext> Collection { get; }

        protected virtual FilterDefinitionBuilder<TContext> FilterBuilder => Builders<TContext>.Filter;

        /// <summary>
        /// Provides abstract generic command instance for NoSqlCollection.
        /// </summary>
        /// <param name="collection"></param>
        public AbsNoSqlAction(INoSqlDBContext<TContext> dBContext, INoSqlSessionProvider session)
        {
            Collection = dBContext.Collection;
            Session = session;
        }

        public abstract TOut Execute(TIn param, IClientSessionHandle session = null);

        public abstract Task<TOut> ExecuteAsync(TIn param, IClientSessionHandle session = null);

        public bool ValidateHash(TContext param)
        {
            if (param is INoSqlSecurityCollection securityParam && !(Session is null))
            {
                string messageToSign = $"{param._id}{Session.AccountId}{Session.SecretHashKey}";
                return securityParam.Hash == XXHash.XXH32(Encoding.ASCII.GetBytes(messageToSign));
            }

            return true;
        }

        public void CalculateHash(TContext obj)
        {
            if (obj is INoSqlSecurityCollection securityParam)
            {
                string messageToSign = $"{obj._id}{Session?.AccountId}{Session?.SecretHashKey}";
                securityParam.Hash = XXHash.XXH32(Encoding.ASCII.GetBytes(messageToSign));
            }
        }
    }
}
