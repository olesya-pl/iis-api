using System.Text.Json;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Iis.MaterialDistributor
{
    internal static class HealthResponseWriter
    {
        private const string ContentType = "application/health+json";
        private static readonly string _appVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        private static readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static Task WriteHealthCheckAsync(HttpContext httpContext, HealthReport healthReport)
        {
            httpContext.Response.ContentType = ContentType;

            var response = JsonSerializer.Serialize(new
            {
                Version = _appVersion,
                Status = healthReport.Status.ToString(),
                Duration = healthReport.TotalDuration.TotalMilliseconds
            }, _serializeOptions);

            return httpContext.Response.WriteAsync(response);
        }
    }
}
