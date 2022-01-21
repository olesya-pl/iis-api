using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.MaterialDistributor.DataModel.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static Task MigrateDatabaseAsync<TDbContext>(this IServiceProvider serviceProvider)
           where TDbContext : DbContext
        {
            var context = serviceProvider.GetService<TDbContext>();

            return context.Database.MigrateAsync();
        }
    }
}