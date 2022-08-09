

using NoSql.MongoDb.Abstraction.Interfaces;

namespace Common.NoSql.AbstractClasses
{
    public abstract class AbsNoSqlQuerying<TIn, Tout, TContext> : AbsNoSqlAction<TIn, Tout, TContext> where TContext : class, INoSqlCollection, new()
    {
        public AbsNoSqlQuerying(INoSqlDBContext<TContext> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }
    }
}
