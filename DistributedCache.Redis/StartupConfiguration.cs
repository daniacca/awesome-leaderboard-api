using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using Common.DistribuitedCache.Manager;
using Common.DistribuitedCache.Interfaces;

namespace Common.DistribuitedCache
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration Configuration)
        {
            var redisConnection = Configuration.GetConnectionString("Redis");
            if (redisConnection is null)
                throw new Exception("Redis connection string not found in configuration file!");

            services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = redisConnection;
            });

            services.AddTransient<IDistributedCacheManager, DistributedCacheManager>();
            return services;
        }
    }
}
