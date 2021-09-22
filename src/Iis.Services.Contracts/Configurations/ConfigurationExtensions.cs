using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.Services.Contracts.Configurations
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration.GetSection("files").Get<FilesConfiguration>());
            services.AddSingleton(configuration.GetSection("upload").Get<UploadConfiguration>());
            return services;
        }
    }
}