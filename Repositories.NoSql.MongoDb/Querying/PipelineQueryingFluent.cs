using Common.NoSql.AbstractClasses;
using NoSql.MongoDb.Abstraction.Interfaces;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NoSql.MongoDb.Abstraction.AbstractClasses;
using NoSql.MongoDb.Querying.PipelineStages;
using Common.Utils.ExtensionMethods;

namespace NoSql.MongoDb.Querying
{
    public class PipelineQueryingFluent<TContext, TOut> : AbsNoSqlPipelineQuerying, IPipelineQueryingFluent<TContext, TOut> where TContext : class, INoSqlCollection, new() where TOut : class
    {
        private INoSqlDBContext<TContext> DbContext { get; }

        private IAggregateFluent<TContext> AggregateFluent { get; set; }

        private IAggregateFluent<TOut> ProjectionAggregate { get; set; } = null;

        public PipelineQueryingFluent(INoSqlDBContext<TContext> dBContext, INoSqlSessionProvider session) : base(session)
        {
            DbContext = dBContext;
            AggregateFluent = DbContext.Collection.Aggregate();
        }

        private void BuildAggregationStages(IEnumerable<AbsAggregationPipelineStage> stages)
        {
            stages.CheckPipeline<TContext, TOut>();
            foreach (var stage in stages)
            {
                switch(stage)
                {
                    case MatchStage<TContext> match:
                        AggregateFluent = AggregateFluent.Match(match.Func);
                        break;
                    case SortStage<TContext> sort:
                        AggregateFluent = AggregateFluent.Sort(sort.Func);
                        break;
                    case SortByStage<TContext> sortBy:
                        AggregateFluent = AggregateFluent.SortBy(sortBy.Func);
                        break;
                    case ProjectStage<TContext, TOut> project:
                        ProjectionAggregate = AggregateFluent.Project(project.Func);
                        break;
                    default:
                        return;
                }
            }
        }

        public IEnumerable<TOut> Get(IEnumerable<AbsAggregationPipelineStage> stages)
        {
            if (!stages.Any())
                return null;

            BuildAggregationStages(stages);

            IEnumerable<TOut> result;
            if (ProjectionAggregate is not null)
                result = ProjectionAggregate.ToList();
            else
                result = AggregateFluent.ToList().Select(i => i as TOut);
            
            result.ToList().ForEach(d => CalculateHash(d));
            return result;
        }

        public async Task<IEnumerable<TOut>> GetAsync(IEnumerable<AbsAggregationPipelineStage> stages)
        {
            if (!stages.Any())
                return null;

            BuildAggregationStages(stages);

            IEnumerable<TOut> result;
            if (ProjectionAggregate is not null)
                result = await ProjectionAggregate.ToListAsync().ConfigureAwait(false);
            else
                result = (await AggregateFluent.ToListAsync().ConfigureAwait(false)).Select(i => i as TOut);

            var tasks = result.Select(p => Task.Run(() => CalculateHash(p)));
            await Task.WhenAll(tasks);
            return result;
        }
    }
}
