using MongoDB.Driver;
using System.ComponentModel;
using System;
using NoSql.MongoDb.Abstraction.Interfaces;
using System.Linq;
using System.Reflection;
using NoSql.MongoDb.Attributes;
using NoSql.MongoDb.ExtensionMethods;

namespace NaTourWine.Core.Repositories.NoSql.Data
{
    public class NoSqlDBContext<T> : INoSqlDBContext<T> where T : class, INoSqlCollection, new()
    {
        public IMongoClientService Client { get; }

        private string CollectionName { get; } = typeof(T).GetAttributeValue((DescriptionAttribute descr) => descr.Description);

        public NoSqlDBContext(IMongoClientService clientService)
        {
            Client = clientService;
            InitCollection();
        }

        protected IMongoDatabase Database => Client.Database;

        public bool CollectionExist => Database.ListCollectionNames().ToList().Any(s => s == CollectionName);

        public bool IsCapped => typeof(T).GetCustomAttribute(typeof(CappedCollectionAttribute)) != null;

        public IMongoCollection<T> Collection => CollectionExist ? Client.Collection<T>(CollectionName) : null;

        private void CreateCollection()
        {
            if (typeof(T).GetCustomAttribute(typeof(CappedCollectionAttribute)) != null)
                Database.CreateCollection(CollectionName, new CreateCollectionOptions 
                { 
                    Capped = true, 
                    MaxSize = typeof(T).GetAttributeValue((CappedCollectionAttribute capped) => capped.Size),
                    MaxDocuments = typeof(T).GetAttributeValue((CappedCollectionAttribute capped) => capped.MaxDocument)
                });
            else
                Database.CreateCollection(CollectionName);   
        }

        private void CreateIndexesWithSingleField()
        {
            var collection = Database.GetCollection<T>(CollectionName);

            var keys = typeof(T).GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(CollectionIndexAttribute)));

            foreach (var k in keys)
            {
                var options = new CreateIndexOptions
                {
                    Unique = k.GetCustomAttribute<CollectionIndexAttribute>()?.Unique ?? false,
                    Name = k.GetCustomAttribute<CollectionIndexAttribute>().Name ?? k.Name,
                    Sparse = k.GetCustomAttribute<CollectionIndexAttribute>()?.Sparse ?? true,
                };

                var ordering = k.GetCustomAttribute<CollectionIndexAttribute>()?.Ordering ?? IndexOrdering.Ascending;

                if (k.PropertyType == typeof(string))
                {
                    var field = new StringFieldDefinition<T>(k.Name);
                    var indexDefinitionBuilder = new IndexKeysDefinitionBuilder<T>();
                    var indexDefinition = ordering switch
                    {
                        IndexOrdering.Ascending => indexDefinitionBuilder.Ascending(field),
                        IndexOrdering.Descending => indexDefinitionBuilder.Descending(field),
                        _ => indexDefinitionBuilder.Ascending(field)
                    };
                    collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(indexDefinition, options)).Wait();
                }
                else
                {
                    IndexKeysDefinition<T> keyCode = $"{{ {k.Name}: {(int)ordering} }}";
                    collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(keyCode, options)).Wait();
                }
            }
        }

        private void CreateIndexesWithMultipleField()
        {
            var collection = Database.GetCollection<T>(CollectionName);

            var keys = typeof(T).GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(CollectionIndexMultipleAttribute)))
                .GroupBy(k => k.GetCustomAttribute<CollectionIndexMultipleAttribute>().Name);

            foreach (var k in keys)
            {
                var options = new CreateIndexOptions
                {
                    Name = k.Key,
                    Unique = k.Select(i => i.GetCustomAttribute<CollectionIndexAttribute>()?.Unique ?? false).Any(v => v),
                    Sparse = k.Select(i => i.GetCustomAttribute<CollectionIndexAttribute>()?.Sparse ?? true).Any(v => !v),
                };

                var propNames = k.Select(i => i.Name).Select(n => $" {n}: 1");
                IndexKeysDefinition<T> keyCode = $"{{ {string.Join(",", propNames)} }}";
                collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(keyCode, options)).Wait();
            }
        }

        private void InitCollection()
        {
            if (!CollectionExist)
            {
                CreateCollection();
                CreateIndexesWithSingleField();
                CreateIndexesWithMultipleField();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}