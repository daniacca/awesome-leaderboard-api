using NoSql.MongoDb.Abstraction.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.ComponentModel;
using NoSql.MongoDb.Types;
using NoSql.MongoDb.ExtensionMethods;
using System.Linq;

namespace NoSql.MongoDb.Tests
{
    public abstract class NoSqlBaseRepositoryTest<TModel, TRepo> : IDisposable where TModel: class, INoSqlCollection, new() where TRepo: INoSqlRepository<TModel>
    {
        #region Private props
        private MongoConnection MongoSettings { get; }
        private MongoClient Mongo { get; }
        private IMongoDatabase Database => Mongo.GetDatabase(MongoSettings.Database);
        private string CollectionName => typeof(TModel).GetAttributeValue((DescriptionAttribute descr) => descr.Description);
        #endregion

        protected TRepo Repository { get; }

        protected abstract bool DropCollection { get; }

        public NoSqlBaseRepositoryTest(TRepo repo, IOptions<MongoConnection> settings)
        {
            MongoSettings = settings.Value;
            Mongo = new MongoClient(settings.Value.ConnectionString);
            Repository = repo;
        }

        protected abstract TModel GenerateTestDocument(int counter = 0);

        protected abstract void FeedDatabase(int document_number);

        public void Dispose()
        {
            if (DropCollection)
            {
                if (Database.ListCollectionNames().ToList().Any(s => s == CollectionName))
                    Database.DropCollection(CollectionName);
            }

            GC.SuppressFinalize(this);
        }
    }
}
