using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Iis.Utility
{
    public class DurationMeter : IDisposable
    {
        private readonly string _message;
        private readonly ILogger _logger;

        private readonly Stopwatch sw;

        private DurationMeter(string message, ILogger logger)
        {
            _message = message;
            _logger = logger;
            sw = new Stopwatch();
            sw.Start();
        }

        public static IDisposable Measure(string message, ILogger logger)
        {
            return new DurationMeter(message, logger);
        }

        public void Dispose()
        {
            sw.Stop();
            _logger.LogInformation($"{_message}. Execution took {sw.ElapsedMilliseconds}");
        }

    }
}
