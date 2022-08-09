using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NoSql.MongoDb.Abstraction.Interfaces;
using NoSql.MongoDb.Types;
using System.Threading.Tasks;

namespace NoSql.MongoDb.Context
{
    class MongoClientService : IMongoClientService
    {
        private string DatabaseName { get; }

        private MongoClient Client { get; }

        public IMongoDatabase Database => Client.GetDatabase(DatabaseName);

        public MongoClientService(IOptions<MongoConnection> settings)
        {
            DatabaseName = settings.Value.Database;
            Client = new MongoClient(settings.Value.ConnectionString);
        }

        public IMongoCollection<T> Collection<T>(string collectionName) => Database.GetCollection<T>(collectionName);

        public IClientSessionHandle StartSession()
        {
            return Client.StartSession();
        }

        public async Task<IClientSessionHandle> StartSessionAsync()
        {
            return await Client.StartSessionAsync();
        }
    }
}
