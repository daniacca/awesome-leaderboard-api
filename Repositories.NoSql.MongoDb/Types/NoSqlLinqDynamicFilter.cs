using NoSql.MongoDb.Abstraction.Interfaces;

namespace NoSql.MongoDb.Types
{
    class NoSqlLinqDynamicFilter : INoSqlLinqDynamicFilter
    {
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }
}
