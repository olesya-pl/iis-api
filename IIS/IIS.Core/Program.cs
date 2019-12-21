using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IIS.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateWebHostBuilder(args).Build();
            if (new ConsoleUtilities(host.Services).ProcessArguments()) // do not start webserver if utilities were started
                host.Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
