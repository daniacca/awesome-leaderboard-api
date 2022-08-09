using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Common.NoSql.AbstractClasses;
using NoSql.MongoDb.Abstraction.Interfaces;

namespace Common.NoSql.Commands
{
    public class UpdateCommand<TIn> : AbsNoSqlCommand<TIn, bool, TIn> where TIn : class, INoSqlCollection, new()
    {
        private readonly ReplaceOptions replaceOptions = new()
        {
            IsUpsert = true,
            BypassDocumentValidation = true
        };

        public UpdateCommand(INoSqlDBContext<TIn> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }

        protected FilterDefinition<TIn> BuildFilter(TIn param)
        {
            var filters = FilterBuilder.Eq(nameof(param._id), param._id);
            return filters;
        }

        public override bool Execute(TIn param, IClientSessionHandle session = null)
        {
            if (!ValidateHash(param))
                throw new UnauthorizedAccessException("Invalid Hash");

            var result = session is null 
                ? Collection.ReplaceOne(BuildFilter(param), param, replaceOptions)
                : Collection.ReplaceOne(session, BuildFilter(param), param, replaceOptions);

            return result.ModifiedCount > 0;
        }

        public override async Task<bool> ExecuteAsync(TIn param, IClientSessionHandle session = null)
        {
            if (!ValidateHash(param))
                throw new UnauthorizedAccessException("Invalid Hash");

            var result = session is null
                ? await Collection.ReplaceOneAsync(BuildFilter(param), param, replaceOptions)
                : await Collection.ReplaceOneAsync(session, BuildFilter(param), param, replaceOptions);
            
            return result.ModifiedCount > 0;
        }
    }
}
