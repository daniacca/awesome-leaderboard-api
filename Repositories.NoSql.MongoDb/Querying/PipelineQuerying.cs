using Common.NoSql.AbstractClasses;
using NoSql.MongoDb.Abstraction.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaTourWine.Core.Repositories.NoSql.Querying
{
    public class PipelineQuerying<TContext, TOutput> : AbsNoSqlPipelineQuerying, IPipelineQuerying<TContext, TOutput> where TContext : class, INoSqlCollection, new()
    {
        private INoSqlDBContext<TContext> DbContext { get; }

        public PipelineQuerying(INoSqlDBContext<TContext> dBContext, INoSqlSessionProvider session) : base(session)
        {
            DbContext = dBContext;
        }

        public IEnumerable<TOutput> Get(IEnumerable<BsonDocument> aggregatePipeline)
        {
            var pipeline = PipelineDefinition<TContext, TOutput>.Create(aggregatePipeline);
            var cursor = DbContext.Collection.Aggregate(pipeline);
            var result = cursor.ToList();
            result.ForEach(d => CalculateHash(d));
            return result;
        }

        public async Task<IEnumerable<TOutput>> GetAsync(IEnumerable<BsonDocument> aggregatePipeline)
        {
            var pipeline = PipelineDefinition<TContext, TOutput>.Create(aggregatePipeline);
            var cursor = await DbContext.Collection.AggregateAsync(pipeline);
            var result = cursor.ToList();
            var tasks = result.Select(p => Task.Run(() => CalculateHash(p)));
            await Task.WhenAll(tasks);
            return result;
        }
    }
}
