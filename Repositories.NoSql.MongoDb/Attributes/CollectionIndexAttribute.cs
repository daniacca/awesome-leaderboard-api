using System;

namespace NoSql.MongoDb.Attributes
{
    public enum IndexOrdering
    {
        Ascending = 1,
        Descending = -1
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CollectionIndexAttribute : Attribute
    {
        public string Name { get; set; }

        public bool Unique { get; set; }

        public bool Sparse { get; set; }

        public IndexOrdering Ordering { get; set; }

        public CollectionIndexAttribute(
            string name = null,
            bool unique = false,
            bool sparse = true,
            IndexOrdering ordering = IndexOrdering.Ascending)
        {
            Name = name;
            Unique = unique;
            Sparse = sparse;
            Ordering = ordering;
        }
    }
}
