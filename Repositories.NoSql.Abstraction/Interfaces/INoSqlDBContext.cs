using MongoDB.Driver;
using System;

namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface INoSqlDBContext<Entity> : IDisposable where Entity : class, INoSqlCollection, new()
    {
        /// <summary>
        /// Returns true if collection exist, false otherwise
        /// </summary>
        bool CollectionExist { get; }

        /// <summary>
        /// Return true if the type <typeparamref name="Entity"/> used 
        /// for the collection is defined as capped, false otherwise
        /// </summary>
        bool IsCapped { get; }

        /// <summary>
        /// Get the Collection reference for this specific context;
        /// If no collection exist yet on the Database, the context
        /// will automatically create it with the specified index;
        /// </summary>
        IMongoCollection<Entity> Collection { get; }

        /// <summary>
        /// Get the Client object which manage connection to the Mongo Database
        /// </summary>
        IMongoClientService Client { get; }
    }
}
