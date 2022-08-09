

using NoSql.MongoDb.Abstraction.Interfaces;

namespace NoSql.MongoDb.Tests
{
    public interface ITestModelRepository : INoSqlRepository<TestModel>
    { }
}
