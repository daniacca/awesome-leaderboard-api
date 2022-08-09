using MongoDB.Driver;
using System.Threading.Tasks;

namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface INoSqlActionAsync<TOut, TIn>
    {
        Task<TOut> ExecuteAsync(TIn param, IClientSessionHandle session = null);
    }
}
