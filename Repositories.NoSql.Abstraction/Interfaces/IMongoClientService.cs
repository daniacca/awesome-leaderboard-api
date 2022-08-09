using MongoDB.Driver;
using System.Threading.Tasks;

namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface IMongoClientService
    {
        IMongoDatabase Database { get; }
        IMongoCollection<T> Collection<T>(string collectionName);
        IClientSessionHandle StartSession();
        Task<IClientSessionHandle> StartSessionAsync();
    }
}
