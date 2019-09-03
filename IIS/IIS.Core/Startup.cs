using System;
using System.Diagnostics;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.Execution;
using HotChocolate.Execution.Batching;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Relay;
using IIS.Core.Files;
using IIS.Core.Files.EntityFramework;
using IIS.Core.Materials;
using IIS.Core.Materials.EntityFramework;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Legacy.EntityFramework;
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
            services.AddMemoryCache();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TraceLoggerProvider());
            var connectionString = Configuration.GetConnectionString("db");
            services.AddDbContext<OntologyContext>(b => b
                .UseNpgsql(connectionString)
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging(),
                ServiceLifetime.Transient);
            services.AddTransient<IOntologyProvider, OntologyProvider>();
            services.AddTransient<IOntologyTypesService, OntologyTypesService>();
            services.AddTransient<ILegacyOntologyProvider, LegacyOntologyProvider>();
            services.AddTransient<IOntologyService, OntologyService>();
            services.AddTransient<OntologyTypeSaver>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IMaterialService, MaterialService>();
            //services.AddSingleton<QueueReanimator>();
            //var mq = Configuration.GetSection("mq").Get<MqConfiguration>();
            //var factory = new ConnectionFactory
            //{
            //    HostName = mq.Host,
            //    UserName = mq.Username,
            //    Password = mq.Password,
            //    RequestedConnectionTimeout = 3 * 60 * 1000, // why this shit doesn't work
            //};
            //services.AddTransient(s => factory);

            services.AddTransient<GraphQL.ISchemaProvider, GraphQL.SchemaProvider>();
            services.AddTransient<GraphQL.Entities.IOntologyFieldPopulator, GraphQL.Entities.OntologyFieldPopulator>();
            services.AddTransient<GraphQL.Entities.Resolvers.IOntologyMutationResolver, GraphQL.Entities.Resolvers.OntologyMutationResolver>();
            services.AddTransient<GraphQL.Entities.Resolvers.IOntologyQueryResolver, GraphQL.Entities.Resolvers.OntologyQueryResolver>();
            services.AddSingleton<GraphQL.Entities.TypeRepository>(); // For HotChocolate ontology types creation. Should have same lifetime as GraphQL schema
            var schema = services.BuildServiceProvider().GetService<GraphQL.ISchemaProvider>().GetSchema();
            // Here it hits the fan. Removed AddGraphQL() method and stripped it to submethods because of IncludeExceptionDetails.
            // todo: remake graphql engine registration in DI
//            services.AddGraphQL(schema);
            QueryExecutionBuilder.BuildDefault(services);
            services.AddTransient<IErrorHandlerOptionsAccessor>(_ => new QueryExecutionOptions {IncludeExceptionDetails = true});
            services.AddSingleton<ISchema>(schema)
                .AddSingleton<IBatchQueryExecutor, BatchQueryExecutor>()
                .AddSingleton<IIdSerializer, IdSerializer>()
                .AddJsonQueryResultSerializer()
                .AddJsonArrayResponseStreamSerializer()
                .AddGraphQLSubscriptions();
            // end of graphql engine registration
            
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll",
            //        builder =>
            //        {
            //            builder
            //            //.AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader()
            //            .AllowCredentials();
            //        });
            //});

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, OntologyContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDeveloperExceptionPage();
            
            //app.UseCors("AllowAll");

            app.Use((ctx, next) =>
            {
                ctx.Response.Headers["Access-Control-Allow-Origin"] = ctx.Request.Headers["Origin"];
                ctx.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                return next.Invoke();
            });

            app.UseGraphQL();
            app.UsePlayground();

            app.UseMvc();

            // MIGRATION IS APPLIED HERE
            context.Database.Migrate();
        }
    }

    public class MqConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
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
