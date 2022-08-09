using NoSql.MongoDb.Abstraction.Interfaces;
using NoSql.MongoDb.Repository;

namespace NoSql.MongoDb.Tests
{
    public class TestModelRepository : NoSqlRepository<TestModel>, ITestModelRepository
    {
        public TestModelRepository(INoSqlDBContext<TestModel> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        {

        }
    }
}
