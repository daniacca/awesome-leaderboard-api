using System;

namespace NoSql.MongoDb.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CollectionIndexMultipleAttribute : Attribute
    {
        public bool Unique { get; set; }

        public string Name { get; set; }

        public bool Sparse { get; set; }

        public CollectionIndexMultipleAttribute(string name, bool unique = false, bool sparse = true)
        {
            Name = name;
            Unique = unique;
            Sparse = sparse;
        }
    }
}
