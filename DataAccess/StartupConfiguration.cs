using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoSql.MongoDb;
using NoSql.MongoDb.Types;

namespace DataAccess;

public static class StartupConfiguration
{
    public static IServiceCollection AddNoSqlRepository(this IServiceCollection services, IConfiguration configuration, bool withSession = true)
    {
        if(withSession)
            services.AddNoSqlRepositoryConfiguration<NoSqlSessionProvider>(configuration);
        else
            services.AddNoSqlRepositoryConfiguration(configuration);

        services.AddTransient<IUserRepository, UserRepository>();
        return services;
    }
}