using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Common.NoSql.AbstractClasses;
using System.Collections.Concurrent;
using NoSql.MongoDb.Abstraction.Interfaces;

namespace Common.NoSql.Commands
{
    public class BulkDeleteCommand<TIn> : AbsNoSqlCommand<IEnumerable<TIn>, bool, TIn> where TIn : class, INoSqlCollection, new()
    {
        public BulkDeleteCommand(INoSqlDBContext<TIn> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }

        protected FilterDefinition<TIn> BuildFilter(IEnumerable<string> param)
        {
            var filters = FilterBuilder.In("_id", param);
            return filters;
        }

        public override bool Execute(IEnumerable<TIn> param, IClientSessionHandle session = null)
        {
            if (!param.All(p => ValidateHash(p)))
                throw new UnauthorizedAccessException("Invalid Hash");

            var query = session is not null 
                ? Collection.DeleteMany(session, BuildFilter(param.Select(p => p._id)))
                : Collection.DeleteMany(BuildFilter(param.Select(p => p._id)));

            return query.DeletedCount > 0;
        }

        public override async Task<bool> ExecuteAsync(IEnumerable<TIn> param, IClientSessionHandle session = null)
        {
            if (!param.All(p => ValidateHash(p)))
                throw new UnauthorizedAccessException("Invalid Hash");

            var idList = new ConcurrentBag<string>();
            var tasks = param.Select(async p => await Task.Run(() => idList.Add(p._id)));
            await Task.WhenAll(tasks);

            var query = session is not null 
                ? await Collection.DeleteManyAsync(session, BuildFilter(idList))
                : await Collection.DeleteManyAsync(BuildFilter(idList));

            return query.DeletedCount > 0;
        }
    }
}
