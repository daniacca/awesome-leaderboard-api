using NoSql.MongoDb.Abstraction.AbstractClasses;
using System;
using System.Linq.Expressions;

namespace NoSql.MongoDb.Querying.PipelineStages
{
    public class ProjectStage<TEntity, TOut> : AbsAggregationPipelineStage<Expression<Func<TEntity, TOut>>>
    {
        public ProjectStage(Expression<Func<TEntity, TOut>> func) : base(func)
        {

        }
    }
}
