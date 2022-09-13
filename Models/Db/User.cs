using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NoSql.MongoDb.Abstraction.Interfaces;
using NoSql.MongoDb.Attributes;

namespace Models.Db;

[Description("users")]
public class User : INoSqlCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id { get; set; }

    [CollectionIndex(Unique = true)]
    public string? Username { get; set; }

    [CollectionIndex(Unique = false)]
    public long? Score { get; set; }
}