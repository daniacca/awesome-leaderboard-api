using Common.NoSql.AbstractClasses;
using NoSql.MongoDb.Abstraction.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace NaTourWine.Core.Repositories.NoSql.Querying
{
    class GetQuerying<TOut> : AbsNoSqlQuerying<string, TOut, TOut>, INoSqlActionAsync<TOut, string> where TOut : class, INoSqlCollection, new()
    {
        public GetQuerying(INoSqlDBContext<TOut> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }

        public override TOut Execute(string param, IClientSessionHandle session = null)
        {
            var data = Collection.Find(Builders<TOut>.Filter.Eq("_id", ObjectId.Parse(param))).FirstOrDefault();
            CalculateHash(data);
            return data;
        }

        public override async Task<TOut> ExecuteAsync(string param, IClientSessionHandle session = null)
        {
            var data = (await Collection.FindAsync(Builders<TOut>.Filter.Eq("_id", ObjectId.Parse(param)))).FirstOrDefault();
            CalculateHash(data);
            return data;
        }
    }
}
