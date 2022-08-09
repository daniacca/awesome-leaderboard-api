using System;

namespace NoSql.MongoDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CappedCollectionAttribute : Attribute
    {
        public long Size { get; set; }

        public long? MaxDocument { get; set; }

        public CappedCollectionAttribute(long size = 5242880, long? maxDocument = null)
        {
            Size = size;
            MaxDocument = maxDocument;
        }
    }
}
