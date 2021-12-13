using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Iis.Desktop.SecurityManager
{
    public class Startup
    {
        public void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Startup>()
                .Build();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("!log.txt")
                .MinimumLevel.Verbose()
                .CreateLogger();
            Log.Information("Started...");
            services.AddSingleton(Log.Logger);
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<MainForm>();
            services.AddAutoMapper(typeof(Startup));
        }
    }
}
