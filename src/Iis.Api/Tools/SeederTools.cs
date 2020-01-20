using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IIS.Core.Tools
{
    internal static class SeederTools
    {
        public static IServiceCollection RegisterSeederTools(this IServiceCollection services) =>
            services
                .AddTransient<UserSeeder>();

        public static async Task SeedUserAsync(this IHost host)
        {
            using IServiceScope scope = host.Services.CreateScope();
            UserSeeder utilities = scope.ServiceProvider.GetRequiredService<UserSeeder>();
            await utilities.SeedDataAsync();
        }
    }
}
