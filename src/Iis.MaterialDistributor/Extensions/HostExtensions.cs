using System.Threading.Tasks;
using Iis.MaterialDistributor.DataModel.Contexts;
using Iis.MaterialDistributor.DataModel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Iis.MaterialDistributor.Extensions
{
    internal static class HostExtensions
    {
        public static async Task MigrateDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            await scope.ServiceProvider.MigrateDatabaseAsync<MaterialDistributorContext>();
        }
    }
}