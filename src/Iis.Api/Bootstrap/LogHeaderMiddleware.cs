using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Iis.Api.Bootstrap
{
    public class LogHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public LogHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<LogHeaderMiddleware>>();

            var header = context.Request.Headers["CorrelationId"];
            var correlationId = header.Count > 0 ? header[0] : Guid.NewGuid().GetHashCode().ToString();
            using (logger.BeginScope("{@CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }
    }
}