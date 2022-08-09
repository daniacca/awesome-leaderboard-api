using MongoDB.Driver;
using NoSql.MongoDb.Abstraction.AbstractClasses;
using System;
using System.Linq.Expressions;

namespace NoSql.MongoDb.Querying.PipelineStages
{
    public class SortStage<T> : AbsAggregationPipelineStage<SortDefinition<T>>
    {
        public SortStage(SortDefinition<T> func) : base(func)
        {

        }
    }

    public class SortByStage<T> : AbsAggregationPipelineStage<Expression<Func<T, object>>>
    {
        public SortByStage(Expression<Func<T, object>> func) : base(func)
        {

        }
    }
}
