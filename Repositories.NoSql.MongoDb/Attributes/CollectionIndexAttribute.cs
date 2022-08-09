using System;

namespace NoSql.MongoDb.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CollectionIndexAttribute : Attribute
    {
        public string Name { get; set; }

        public bool Unique { get; set; }

        public bool Sparse { get; set; }

        public CollectionIndexAttribute(string name = null, bool unique = false, bool sparse = true)
        {
            Name = name;
            Unique = unique;
            Sparse = sparse;
        }
    }
}
