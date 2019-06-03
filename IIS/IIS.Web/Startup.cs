using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GraphiQl;
using GraphQL.DataLoader;
using IIS.Core;
using IIS.Core.Ontology;
using IIS.Core.Resolving;
using IIS.Introspection;
using IIS.Ontology.EntityFramework;
using IIS.Search;
using IIS.Search.Resolving;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using RabbitMQ.Client;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace IIS.Web
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
            services.AddSingleton<IOSchema, SchemaRepository>();
            services.AddSingleton<IOntology, OntologyRepository>();
            services.AddSingleton<IGraphQLSchemaProvider, GraphQLSchemaProvider>();
            services.AddSingleton<IDataLoaderContextAccessor, DataLoaderContextAccessor>();
            services.AddSingleton<DataLoaderDocumentListener>();
            services.AddSingleton<QueueReanimator>();
            services.AddSingleton<IDictionary<string, IRelationResolver>>(s => new Dictionary<string, IRelationResolver>
            {
                ["relationInfo"] = null,
                ["attribute"] = new AttributeResolver(),
                ["entities"] = new EntitiesResolver(s.GetRequiredService<OntologySearchService>(), s.GetRequiredService<IDataLoaderContextAccessor>()),//new EntitiesResolver(s.GetRequiredService<IOntology>(), s.GetRequiredService<IDataLoaderContextAccessor>()),
                ["entityRelation"] = new EntityRelationResolver(s.GetRequiredService<OntologySearchService>(), s.GetRequiredService<IDataLoaderContextAccessor>()),
            });

            // mq
            var factory = new ConnectionFactory() { HostName = "mq" };
            services.AddTransient(s => factory);

            var es = Configuration.GetConnectionString("es");
            var node = new Uri(es);
            var settings = new ConnectionSettings(node)
                .ThrowExceptions();
            services.AddSingleton<IElasticClient>(s => new ElasticClient(settings));

            // search
            services.AddTransient<OntologySearchService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseGraphiQl("/graphiql", "/api/graph");

            app.UseMvc();
        }
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
