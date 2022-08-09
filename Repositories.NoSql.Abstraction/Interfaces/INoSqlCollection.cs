using MongoDB.Bson;

namespace NoSql.MongoDb.Abstraction.Interfaces
{
    /// <summary>
    /// Identifies Entity Model class which keeps
    /// model for NoSql DB collections.
    /// </summary>
    public interface INoSqlCollection
    {
        string _id { get; set; }
    }
}
