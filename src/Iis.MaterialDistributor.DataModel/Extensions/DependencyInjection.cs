using System;
using Iis.MaterialDistributor.DataModel.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Iis.MaterialDistributor.DataModel.Extensions
{
    public static class DependencyInjection
    {
        private const int RetryCount = 3;
        private const int PoolSize = 1024;

        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        public static IServiceCollection AddMaterialDistributorContext(
            this IServiceCollection services,
            string connectionStringName,
            int retryCount = RetryCount,
            TimeSpan? timeout = default)
        {
            services.AddDbContextPool<MaterialDistributorContext>(
                (serviceProvider, optionsBuilder) =>
                {
                    string connectionString = serviceProvider
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString(connectionStringName);

                    optionsBuilder
                        .UseNpgsql(connectionString, optionsBuilder =>
                        {
                            optionsBuilder
                                .EnableRetryOnFailure(retryCount, timeout ?? Timeout, null)
                                .MigrationsAssembly(typeof(MaterialDistributorContext).Assembly.FullName);
                        })
                        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
#if DEBUG
                    optionsBuilder.EnableSensitiveDataLogging(true);
#endif
                }, PoolSize);

            return services;
        }
    }
}