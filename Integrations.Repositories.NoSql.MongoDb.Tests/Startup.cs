using NoSql.MongoDb.Abstraction.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace NoSql.MongoDb.Tests
{
    public class Startup
    {
        // Fix class used for testing for INoSqlSessionProvider
        internal class TestingSessionProvider : INoSqlSessionProvider
        {
            public string AccountId => "admin";
            public string SecretHashKey => "Sup35Sec3e_T";
        }

        public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
        {
            // Configure required services
            services
                .AddNoSqlRepositoryConfiguration<TestingSessionProvider>(context.Configuration)
                .AddTransient<ITestModelRepository, TestModelRepository>();
        }

        public void ConfigureHost(IHostBuilder hostBuilder) =>
            hostBuilder
            .ConfigureHostConfiguration(builder =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            }).ConfigureAppConfiguration((context, builder) =>
            {
                context.Configuration = builder.Build();
            });
    }
}
