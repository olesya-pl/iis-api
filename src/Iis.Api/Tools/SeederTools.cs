using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IIS.Core.Tools
{
    internal static class SeederTools
    {
        public static IServiceCollection RegisterSeederTools(this IServiceCollection services) =>
            services
                .AddTransient<UserSeeder>();
        
        public static void SeedUser(this IApplicationBuilder host)
        {
            using IServiceScope scope = host.ApplicationServices.CreateScope();
            UserSeeder utilities = scope.ServiceProvider.GetRequiredService<UserSeeder>();
            utilities.SeedDataAsync().GetAwaiter().GetResult();
        }
    }
}
