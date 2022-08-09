namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface INoSqlSecurityCollection : INoSqlCollection
    {
        uint Hash { get; set; }
    }
}
