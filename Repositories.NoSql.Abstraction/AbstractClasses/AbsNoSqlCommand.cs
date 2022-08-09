using NoSql.MongoDb.Abstraction.Interfaces;

namespace Common.NoSql.AbstractClasses
{
    public abstract class AbsNoSqlCommand<TIn, Tout, TContext> : AbsNoSqlAction<TIn, Tout, TContext> where TContext : class, INoSqlCollection, new()
    {
        protected AbsNoSqlCommand(INoSqlDBContext<TContext> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }
    }
}
