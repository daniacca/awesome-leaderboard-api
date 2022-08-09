using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Common.NoSql.AbstractClasses;
using System.Collections.Concurrent;
using NoSql.MongoDb.Abstraction.Interfaces;
using MongoDB.Bson;

namespace Common.NoSql.Commands
{
    public class BulkUpdateCommand<TIn> : AbsNoSqlCommand<IEnumerable<TIn>, bool, TIn> where TIn : class, INoSqlCollection, new()
    {
        public BulkUpdateCommand(INoSqlDBContext<TIn> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }

        private FilterDefinition<TIn> BuildFilter(TIn param)
        {
            var filters = FilterBuilder.Eq(nameof(param._id), param._id);
            return filters;
        }

        public override bool Execute(IEnumerable<TIn> param, IClientSessionHandle session = null)
        {
            if (!param.All(p => p._id is null || ValidateHash(p)))
                throw new UnauthorizedAccessException("Invalid Hash");

            var updates = param.Select(doc =>
            {
                WriteModel<TIn> toWrite;
                if (doc._id is null)
                    toWrite = new InsertOneModel<TIn>(doc);
                else
                    toWrite = new ReplaceOneModel<TIn>(BuildFilter(doc), doc);
                return toWrite;
            });

            var options = new BulkWriteOptions { IsOrdered = false };
            var result = session is null
                ? Collection.BulkWrite(updates, options)
                : Collection.BulkWrite(session, updates, options);

            return result.ModifiedCount > 0 || result.InsertedCount > 0;
        }

        public override async Task<bool> ExecuteAsync(IEnumerable<TIn> param, IClientSessionHandle session = null)
        {
            if (!param.All(p => p._id is null || ValidateHash(p)))
                throw new UnauthorizedAccessException("Invalid Hash");

            var updates = new ConcurrentBag<WriteModel<TIn>>();
            var tasks = param.Select(async doc =>
                    await Task.Run(() =>
                    {
                        if (doc._id is null)
                            updates.Add(new InsertOneModel<TIn>(doc));
                        else
                            updates.Add(new ReplaceOneModel<TIn>(BuildFilter(doc), doc));
                    }));

            await Task.WhenAll(tasks);

            var options = new BulkWriteOptions { IsOrdered = false };
            var result = session is null
                ? await Collection.BulkWriteAsync(updates, options)
                : await Collection.BulkWriteAsync(session, updates, options);

            return result.ModifiedCount > 0 || result.InsertedCount > 0;
        }
    }
}
