using NoSql.MongoDb.Abstraction.AbstractClasses;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using NoSql.MongoDb.Querying.PipelineStages;
using MongoDB.Driver;

namespace Common.Utils.ExtensionMethods
{
    public static class IEnumerablePipelineExtensions
    {
        public static IEnumerable<AbsAggregationPipelineStage> Match<T>(this IEnumerable<AbsAggregationPipelineStage> stages, Expression<Func<T, bool>> func)
        {
            return stages.Append(new MatchStage<T>(func));
        }

        public static IEnumerable<AbsAggregationPipelineStage> Sort<T>(this IEnumerable<AbsAggregationPipelineStage> stages, SortDefinition<T> func)
        {
            return stages.Append(new SortStage<T>(func));
        }

        public static IEnumerable<AbsAggregationPipelineStage> SortBy<T>(this IEnumerable<AbsAggregationPipelineStage> stages, Expression<Func<T, object>> func)
        {
            return stages.Append(new SortByStage<T>(func));
        }

        public static IEnumerable<AbsAggregationPipelineStage> Project<TEntity, TOut>(this IEnumerable<AbsAggregationPipelineStage> stages, Expression<Func<TEntity, TOut>> func)
        {
            return stages.Append(new ProjectStage<TEntity, TOut>(func));
        }

        public static bool IsLast<T>(this IEnumerable<T> items, T item)
        {
            var last = items.LastOrDefault();
            if (last == null)
                return false;
            return item.Equals(last);
        }

        public static void CheckPipeline<TContext, TOut>(this IEnumerable<AbsAggregationPipelineStage> stages)
        {
            var projectStages = stages.Count(s => s is ProjectStage<TContext, TOut>);
            if (projectStages > 1)
                throw new ArgumentException($"{nameof(stages)} must contain only one project stage!");

            if (stages.TakeLast(1) is ProjectStage<TContext, TOut>)
                throw new ArgumentException($"{nameof(stages)} must have the project stage as last stage!");
        }
    }
}
