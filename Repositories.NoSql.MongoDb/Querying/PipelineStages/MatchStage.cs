using NoSql.MongoDb.Abstraction.AbstractClasses;
using System;
using System.Linq.Expressions;

namespace NoSql.MongoDb.Querying.PipelineStages
{
    public class MatchStage<TEntity> : AbsAggregationPipelineStage<Expression<Func<TEntity, bool>>>
    {
        public MatchStage(Expression<Func<TEntity, bool>> func): base(func)
        {
            
        }
    }
}
