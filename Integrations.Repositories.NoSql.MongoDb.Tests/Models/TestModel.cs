using NoSql.MongoDb.Abstraction.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel;
using NoSql.MongoDb.Attributes;

namespace NoSql.MongoDb.Tests
{
    public static class NoSQLTestConstants
    {
        public const string Collection = "Test_NoSQL_Repositories";
    }

    public enum Placeholder
    {
        Ichi,
        Nii,
        San
    }

    public enum TestEnum
    {
        Item0,
        Item1,
        Item2,
    }

    public class InnerTwo
    {
        public int TestNumberInnerTwo { get; set; }
        public string TestStringInnerTwo { get; set; }
    }

    public class InnerOne
    {
        public int TestNumberInnerOne { get; set; }
        public string TestStringInnerOne { get; set; }
        public InnerTwo InnerTree { get; set; }
    }

    [Description(NoSQLTestConstants.Collection)]
    public class TestModel : INoSqlSecurityCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string TestString { get; set; }

        public long TestNumber { get; set; }

        public TestEnum TestEnumerator { get; set; }

        public DateTime TestDate { get; set; }

        public InnerOne TestTreeRoot { get; set; }

        [CollectionIndex]
        public int TestIndex { get; set; }

        [CollectionIndex(Unique = true)]
        [BsonRepresentation(BsonType.String)]
        public Guid TestUniqueGuidIndex { get; set; }

        [BsonIgnore]
        public uint Hash { get; set; }
    }
}
