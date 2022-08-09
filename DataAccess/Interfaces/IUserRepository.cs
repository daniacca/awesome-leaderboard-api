using Models.Db;
using NoSql.MongoDb.Abstraction.Interfaces;

namespace DataAccess.Interfaces;

public interface IUserRepository : INoSqlRepository<User>
{
}