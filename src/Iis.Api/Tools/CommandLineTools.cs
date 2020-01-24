using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IIS.Core.Tools
{
    internal static class CommandLineTools
    {
        public static IServiceCollection RegisterRunUpTools(this IServiceCollection services) =>
            services
                .AddTransient<RunUpTools>()
                .AddTransient<ActionTools>()
                .AddTransient<ElasticTools>();

        public static async Task<bool> RunUpAsync(this IHost host)
        {
            using IServiceScope scope = host.Services.CreateScope();
            RunUpTools utilities = scope.ServiceProvider.GetRequiredService<RunUpTools>();
            return await utilities.ProcessArgumentsAsync();
        }
    }
}
