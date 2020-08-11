using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Iis.Services;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Interfaces;

namespace Iis.Api.Bootstrap
{
    public class LoggingMiddleware
    {
        readonly RequestDelegate _next;
        private static ILogger<LogHeaderMiddleware> logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly ISanitizeService _sanitizeService;

        public LoggingMiddleware(RequestDelegate next, ISanitizeService sanitizeService)
        {
            _next = next;
            _sanitizeService = sanitizeService;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            logger = context.RequestServices.GetRequiredService<ILogger<LogHeaderMiddleware>>();

            if (context == null) throw new ArgumentNullException(nameof(context));

            var sw = Stopwatch.StartNew();
            try
            {
                var request = await FormatRequest(context.Request);
                logger.LogInformation(request);

                var originalBodyStream = context.Response.Body;
                await using var responseBody = _recyclableMemoryStreamManager.GetStream();
                context.Response.Body = responseBody;
                await _next(context);

                var response = await FormatResponse(context.Response, sw);

                logger.LogInformation(response);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex) when (LogException(sw, ex)) { }
        }

        private static string ReadStreamInChunks(Stream requestStream)
        {
            string requestBody;
            requestStream.Position = 0;
            using (StreamReader streamReader = new StreamReader(requestStream))
            {
                requestBody = streamReader.ReadToEnd();
            }
            return requestBody;
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var isFileRequest = request.Path.Value.ToLower().Contains("files");
            var requestBodyString = string.Empty;
            if (!isFileRequest)
            {
                await using var requestStream = _recyclableMemoryStreamManager.GetStream();
                await request.Body.CopyToAsync(requestStream);
                requestBodyString = _sanitizeService.SanitizeBody(ReadStreamInChunks(requestStream));
                request.Body.Position = 0;
            }
            var dump = $"Request: {request.Scheme} {request.Host}{request.Path} {request.QueryString} {requestBodyString}";
            return dump;
        }

        private async Task<string> FormatResponse(HttpResponse response, Stopwatch sw)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return $"Response: {response.StatusCode}: {_sanitizeService.SanitizeBody(text)} Elapsed(ms): {sw.ElapsedMilliseconds}";
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