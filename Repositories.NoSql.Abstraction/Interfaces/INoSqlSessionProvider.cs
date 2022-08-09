namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface INoSqlSessionProvider
    {
        string AccountId { get; }
        string SecretHashKey { get; }
    }
}
