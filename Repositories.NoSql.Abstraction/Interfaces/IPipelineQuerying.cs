using MongoDB.Bson;
using NoSql.MongoDb.Abstraction.AbstractClasses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface IPipelineQuerying<TContext, TOutput>
    {
        IEnumerable<TOutput> Get(IEnumerable<BsonDocument> aggregatePipeline);
        Task<IEnumerable<TOutput>> GetAsync(IEnumerable<BsonDocument> aggregatePipeline);
    }

    public interface IPipelineQueryingFluent<TContext, TOutput>
    {
        IEnumerable<TOutput> Get(IEnumerable<AbsAggregationPipelineStage> stages);
        Task<IEnumerable<TOutput>> GetAsync(IEnumerable<AbsAggregationPipelineStage> stages);
    }
}
