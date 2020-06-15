using System;
using System.Threading.Tasks;
using IIS.Core.Tools;
using Microsoft.AspNetCore.Hosting;
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
                await host.SeedUserAsync();
                host.Run();
            }
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
