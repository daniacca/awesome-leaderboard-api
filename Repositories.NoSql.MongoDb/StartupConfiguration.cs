using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using NaTourWine.Core.Repositories.NoSql.Data;
using NoSql.MongoDb.Abstraction.Interfaces;
using NaTourWine.Core.Repositories.NoSql.Querying;
using NoSql.MongoDb.Context;
using NoSql.MongoDb.NameConventions;
using NoSql.MongoDb.Repository;
using NoSql.MongoDb.Types;
using Repositories.NoSql.MongoDb.Types;

namespace NoSql.MongoDb
{
    public static class StartupConfiguration
    {
        private static IServiceCollection AddCommonService(IServiceCollection services, IConfiguration configuration, bool withCamelCase = true)
        {
            if (withCamelCase)
            {
                var pack = new ConventionPack { new CamelCaseNameConvention() };
                ConventionRegistry.Register("CamelCase", pack, _ => true);
            }

            services
                .Configure<MongoConnection>(option => configuration.GetSection(nameof(MongoConnection)).Bind(option))
                .AddSingleton<IMongoClientService, MongoClientService>()
                .AddTransient(typeof(IPipelineQuerying<,>), typeof(PipelineQuerying<,>))
                .AddTransient(typeof(INoSqlDBContext<>), typeof(NoSqlDBContext<>))
                .AddTransient(typeof(INoSqlRepository<>), typeof(NoSqlRepository<>));

            return services;
        }

        public static IServiceCollection AddNoSqlRepositoryConfiguration<TSession>(this IServiceCollection services, IConfiguration configuration, bool withCamelCase = true)
            where TSession : class, INoSqlSessionProvider
        {
            services = AddCommonService(services, configuration, withCamelCase);
            return services.AddHttpContextAccessor().AddScoped<INoSqlSessionProvider, TSession>();
        }

        public static IServiceCollection AddNoSqlRepositoryConfiguration(this IServiceCollection services, IConfiguration configuration, bool withCamelCase = true)
        {
            services = AddCommonService(services, configuration, withCamelCase);
            return services.AddSingleton<INoSqlSessionProvider, FixedSessionProvider>();
        }
    }
}
