using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace IIS.Core
{
    public class Program
    {
        public static bool IsStartedFromMain { get; set; }
        public static bool NeedToStart { get; set; } = true;
        public static async Task Main(string[] args)
        {
            IsStartedFromMain = true;
            while (NeedToStart)
            {
                try
                {
                    NeedToStart = false;
                    IHost host = CreateWebHostBuilder(args).Build();
                    host.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}; {ex.InnerException?.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables(prefix: "IisApi_");
                })
                .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
