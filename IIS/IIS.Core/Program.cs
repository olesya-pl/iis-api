using System.Threading.Tasks;
using IIS.Core.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IIS.Core
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = CreateWebHostBuilder(args).Build();
            if (await host.RunUpAsync())
            {
                host.Run();
            }
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
