using DataAccess.Interfaces;
using Models.Db;
using NoSql.MongoDb.Abstraction.Interfaces;
using NoSql.MongoDb.Repository;

namespace DataAccess.Repositories;

public class UserRepository : NoSqlRepository<User>, IUserRepository
{
	public UserRepository(INoSqlDBContext<User> dBContext, INoSqlSessionProvider session)
		: base(dBContext, session)
	{
	}
}