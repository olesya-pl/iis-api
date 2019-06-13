using System;
using System.Collections.Generic;
using System.Diagnostics;
using IIS.Core.Ontology;
using IIS.Core.Schema;
using IIS.Introspection;
using IIS.Ontology.EntityFramework;
using IIS.Schema.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace IIS.Core
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TraceLoggerProvider());
            var connectionString = Configuration.GetConnectionString("db");
            services.AddDbContext<ContourContext>(b => b.UseNpgsql(connectionString).UseLoggerFactory(loggerFactory), ServiceLifetime.Singleton);
            services.AddTransient<ISchemaProvider, SchemaProvider>();
            services.AddSingleton<IOSchema, SchemaRepository>();
            services.AddSingleton<IOntology, OntologyRepository>();
            services.AddSingleton<IDictionary<string, IRelationResolver>>(s => new Dictionary<string, IRelationResolver>
            {
                ["relationInfo"] = null,
                ["attribute"] = null,
                ["entities"] = null,
                ["entityRelation"] = null,
            });
            services.AddSingleton<QueueReanimator>();
            var mq = Configuration.GetSection("mq").Get<MqConfiguration>();
            var factory = new ConnectionFactory
            {
                HostName = mq.Host,
                UserName = mq.Username,
                Password = mq.Password,
                RequestedConnectionTimeout = 3 * 60 * 1000, // why this shit doesn't work
            };
            services.AddTransient(s => factory);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }

    public class MqConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class EsConfiguration
    {
        public string Host { get; set; }
    }

    public class TraceLogger : ILogger
    {
        private readonly string categoryName;

        public TraceLogger(string categoryName) => this.categoryName = categoryName;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Trace.WriteLine($"{DateTime.Now.ToString("o")} {logLevel} {eventId.Id} {this.categoryName}");
            Trace.WriteLine(formatter(state, exception));
        }

        public IDisposable BeginScope<TState>(TState state) => null;
    }

    public class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName);

        public void Dispose() { }
    }
}
