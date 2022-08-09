namespace NoSql.MongoDb.Abstraction.AbstractClasses
{
    public abstract class AbsAggregationPipelineStage
    { }

    public abstract class AbsAggregationPipelineStage<TStageInput> : AbsAggregationPipelineStage
    {
        public TStageInput Func { get; }

        protected AbsAggregationPipelineStage(TStageInput func)
        {
            Func = func;
        }
    }
}
