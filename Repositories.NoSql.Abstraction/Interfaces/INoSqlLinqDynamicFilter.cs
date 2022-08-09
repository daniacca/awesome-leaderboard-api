namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface INoSqlLinqDynamicFilter
    {
        string Where { get; set; }
        string OrderBy { get; set; }
        int? PageSize { get; set; }
        int? PageNumber { get; set; }
    }
}
