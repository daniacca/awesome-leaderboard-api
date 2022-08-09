using MongoDB.Driver;

namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface INoSqlAction<TOut, TIn>
    {
        TOut Execute(TIn param, IClientSessionHandle session = null);
    }
}
