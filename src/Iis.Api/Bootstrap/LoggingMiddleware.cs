using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Iis.Api.Bootstrap
{
    public class LoggingMiddleware
    {
        readonly RequestDelegate _next;
        private static ILogger<LoggingMiddleware> logger;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            logger = context.RequestServices.GetRequiredService<ILogger<LoggingMiddleware>>();

            if (context == null) throw new ArgumentNullException(nameof(context));

            var sw = Stopwatch.StartNew();
            try
            {
                var request = FormatRequest(context.Request);
                logger.LogInformation(request);
                var originalBodyStream = context.Response.Body;

                await using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                var response = await FormatResponse(context.Response, sw);

                logger.LogInformation(response);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex) when (LogException(sw, ex)) { }
        }

        private string FormatRequest(HttpRequest request)
        {
            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString}";
        }

        private async Task<string> FormatResponse(HttpResponse response, Stopwatch sw)
        {
            response.Body.Seek(0, SeekOrigin.Begin);

            string text = await new StreamReader(response.Body).ReadToEndAsync();

            response.Body.Seek(0, SeekOrigin.Begin);
            sw.Stop();
            return $"{response.StatusCode}: {text} Elapsed(ms): {sw.ElapsedMilliseconds}";
        }

        static bool LogException(Stopwatch sw, Exception ex)
        {
            sw.Stop();
            //TODO. add additional exception logging
            //logger.LogError(ex,);
            
            return false;
        }
    }
}