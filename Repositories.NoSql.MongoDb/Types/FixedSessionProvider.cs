using NoSql.MongoDb.Abstraction.Interfaces;

namespace Repositories.NoSql.MongoDb.Types;

public class FixedSessionProvider : INoSqlSessionProvider
{
    public string AccountId { get; } = "Fixed";

    public string SecretHashKey { get; } = "Sup35Sec3e_T";
}
