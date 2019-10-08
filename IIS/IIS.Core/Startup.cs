﻿using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.Execution;
using HotChocolate.Execution.Batching;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Relay;
using IIS.Core.Files;
using IIS.Core.Files.EntityFramework;
using IIS.Core.GSM.Consumer;
using IIS.Core.GSM.Producer;
using IIS.Core.Materials;
using IIS.Core.Materials.EntityFramework;
using IIS.Core.Ontology;
using IIS.Core.Ontology.ComputedProperties;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Legacy.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

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

            var connectionString = Configuration.GetConnectionString("db");
            services.AddDbContext<OntologyContext>(b => b
                .UseNpgsql(connectionString)
                //.EnableSensitiveDataLogging()
                ,
                ServiceLifetime.Scoped);

            services.AddHttpContextAccessor();
            services.AddSingleton<IOntologyProvider, OntologyProvider>();
            services.AddTransient<Ontology.Seeding.Odysseus.TypeSeeder>();
            services.AddTransient<ILegacyOntologyProvider, LegacyOntologyProvider>();
            services.AddTransient<ILegacyMigrator, LegacyMigrator>();
            services.AddTransient<IOntologyService, OntologyService>();
            services.AddTransient<OntologyTypeSaver>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IMaterialProvider, MaterialProvider>();
            services.AddTransient<IMaterialService, MaterialService>();

            // material processors
            services.AddTransient<IMaterialProcessor, Materials.EntityFramework.Workers.MetadataExtractor>();
            services.AddTransient<IMaterialProcessor, Materials.EntityFramework.Workers.Odysseus.PersonForm5Processor>();

            services.AddTransient<Ontology.Seeding.Seeder>();
            services.AddTransient(e => new ContextFactory(connectionString));
            services.AddTransient(e => new FileServiceFactory(connectionString));
            services.AddTransient<IComputedPropertyResolver, ComputedPropertyResolver>();

            services.AddTransient<GraphQL.ISchemaProvider, GraphQL.SchemaProvider>();
            services.AddTransient<GraphQL.Entities.IOntologyFieldPopulator, GraphQL.Entities.OntologyFieldPopulator>();
            services.AddTransient<GraphQL.Entities.Resolvers.IOntologyMutationResolver, GraphQL.Entities.Resolvers.OntologyMutationResolver>();
            services.AddTransient<GraphQL.Entities.Resolvers.IOntologyQueryResolver, GraphQL.Entities.Resolvers.OntologyQueryResolver>();
            services.AddSingleton<GraphQL.Entities.TypeRepository>(); // For HotChocolate ontology types creation. Should have same lifetime as GraphQL schema
            // Here it hits the fan. Removed AddGraphQL() method and stripped it to submethods because of IncludeExceptionDetails.
            // todo: remake graphql engine registration in DI
//            services.AddGraphQL(schema);
            QueryExecutionBuilder.BuildDefault(services);
            services.AddTransient<IErrorHandlerOptionsAccessor>(_ => new QueryExecutionOptions {IncludeExceptionDetails = true});
            services.AddSingleton(s => s.GetService<GraphQL.ISchemaProvider>().GetSchema())
                .AddSingleton<IBatchQueryExecutor, BatchQueryExecutor>()
                .AddSingleton<IIdSerializer, IdSerializer>()
                .AddJsonQueryResultSerializer()
                .AddJsonArrayResponseStreamSerializer()
                .AddGraphQLSubscriptions();
            // end of graphql engine registration
            services.AddDataLoaderRegistry();

            var mq = Configuration.GetSection("mq").Get<MqConfiguration>();
            if (mq == null) throw new Exception("mq config not found");
            var factory = new ConnectionFactory
            {
                HostName = mq.Host,
                UserName = mq.Username,
                Password = mq.Password,
                RequestedConnectionTimeout = 3 * 60 * 1000, // why this shit doesn't work
            };
            services.AddTransient<IConnectionFactory>(s => factory);

            var gsmWorkerUrl = Configuration.GetValue<string>("gsmWorkerUrl");
            services.AddSingleton<IGsmTranscriber>(e => new GsmTranscriber(gsmWorkerUrl));
            services.AddSingleton<IMaterialEventProducer, MaterialEventProducer>();
            services.AddHostedService<MaterialEventConsumer>();


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

            app.UseMiddleware<OptionsMiddleware>();

            app.UseGraphQL();
            app.UsePlayground();

            app.UseMvc();
        }
    }

    public class MqConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class OptionsMiddleware
    {
        private readonly RequestDelegate _next;

        public OptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            return BeginInvoke(context);
        }

        private Task BeginInvoke(HttpContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)context.Request.Headers["Origin"] });
            context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "authorization,content-type" });
            context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });
            context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
            context.Response.Headers.Add("Access-Control-Max-Age", new[] { "600" });

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                return Task.FromResult(context.Response);
            }

            return _next.Invoke(context);
        }
    }
}
