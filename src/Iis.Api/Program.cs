using System;
using System.Threading.Tasks;
using IIS.Core.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace IIS.Core
{
    public class Program
    {
        public static bool IsStartedFromMain { get; set; }
        public static async Task Main(string[] args)
        {
            IsStartedFromMain = true;
            IHost host = CreateWebHostBuilder(args).Build();
            if (await host.RunUpAsync())
            {
                host.Run();
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
