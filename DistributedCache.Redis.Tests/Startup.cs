using Common.DistribuitedCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Common.DistributedCache.Tests
{
    public class Startup
    {
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

        public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
        {
            services.AddRedisCache(context.Configuration);
        }
    }
}
