using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Security.Authentication;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.Execution;
using HotChocolate.Execution.Batching;
using HotChocolate.Execution.Configuration;
using HotChocolate.Language;
using HotChocolate.Types.Relay;
using IIS.Core.Files;
using IIS.Core.Files.EntityFramework;
using IIS.Core.Materials;
using IIS.Core.Materials.EntityFramework;
using IIS.Core.Ontology;
using IIS.Core.Ontology.ComputedProperties;
using IIS.Core.Ontology.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.Tools;
using Iis.DataModel;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using Iis.Domain.Elastic;
using Iis.Elastic;
using Iis.Api;
using Iis.Api.Configuration;
using Microsoft.Extensions.Logging;
using Iis.Api.Ontology.Migration;
using AutoMapper;
using Iis.Api.Export;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using IIS.Domain;
using Iis.Domain;
using Iis.DbLayer.Ontology.EntityFramework;

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
            services
                .RegisterRunUpTools()
                .RegisterSeederTools()
                .AddConfigurations(Configuration);

            services.AddMemoryCache();

            var dbConnectionString = Configuration.GetConnectionString("db", "DB_");
            services.AddDbContext<OntologyContext>(options => options
                .UseNpgsql(dbConnectionString)
            // .EnableSensitiveDataLogging()
            );

            services.AddHttpContextAccessor();
            services.AddSingleton<IOntologyProvider, OntologyProvider>();
            services.AddTransient<IOntologyService, OntologyService>();
            services.AddTransient<IExtNodeService, ExtNodeService>();
            services.AddTransient<OntologyTypeSaver>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IMaterialProvider, MaterialProvider>();
            services.AddTransient<IMaterialService, MaterialService>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
            services.AddTransient<IElasticService, ElasticService>();
            services.AddTransient<MigrationService>();
            services.AddSingleton<RunTimeSettings>();
            services.AddScoped<ExportService>();

            // material processors
            services.AddTransient<IMaterialProcessor, Materials.EntityFramework.Workers.MetadataExtractor>();
            services.AddTransient<IMaterialProcessor, Materials.EntityFramework.Workers.Odysseus.PersonForm5Processor>();

            services.AddTransient<Ontology.Seeding.Seeder>();
            services.AddTransient(e => new ContextFactory(dbConnectionString));
            services.AddTransient(e => new FileServiceFactory(dbConnectionString, e.GetService<FilesConfiguration>(), e.GetService<ILogger<FileService>>()));
            services.AddTransient<IComputedPropertyResolver, ComputedPropertyResolver>();

            services.AddTransient<GraphQL.ISchemaProvider, GraphQL.SchemaProvider>();
            services.AddTransient<GraphQL.Entities.IOntologyFieldPopulator, GraphQL.Entities.OntologyFieldPopulator>();
            services.AddTransient<GraphQL.Entities.Resolvers.IOntologyMutationResolver, GraphQL.Entities.Resolvers.OntologyMutationResolver>();
            services.AddTransient<GraphQL.Entities.Resolvers.IOntologyQueryResolver, GraphQL.Entities.Resolvers.OntologyQueryResolver>();
            services.AddSingleton<GraphQL.Entities.TypeRepository>(); // For HotChocolate ontology types creation. Should have same lifetime as GraphQL schema
            // Here it hits the fan. Removed AddGraphQL() method and stripped it to submethods because of IncludeExceptionDetails.
            // todo: remake graphql engine registration in DI
            //services.AddGraphQL(schema);


            var publiclyAccesible = new HashSet<string> { "login", "__schema" };
            QueryExecutionBuilder.New()
                .Use(next => context =>
                {
                    try
                    {
                        // _authenticate(context, publiclyAccesible);
                    }
                    catch (Exception e)
                    {
                        if (!(e is AuthenticationException) && !(e is InvalidOperationException))
                            throw e;

                        var errorHandler = context.Services.GetService<IErrorHandler>();
                        var error = ErrorBuilder.New()
                            .SetMessage(e.Message)
                            .SetException(e)
                            .Build();
                        context.Exception = e;
                        context.Result = QueryResult.CreateError(errorHandler.Handle(error));

                        return Task.FromResult(context);
                    }

                    return next(context);
                })
                .UseDefaultPipeline()
                .AddErrorFilter<AppErrorFilter>()
                .Populate(services);

            services.AddTransient<IErrorHandlerOptionsAccessor>(_ => new QueryExecutionOptions { IncludeExceptionDetails = true });
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
                RequestedConnectionTimeout = 3 * 60 * 1000, // why this shit doesn't work
            };
            if (mq.Username != null)
            {
                factory.UserName = mq.Username;
            }
            if (mq.Password != null)
            {
                factory.Password = mq.Password;
            }

            services.AddTransient<IConnectionFactory>(s => factory);

            string mqString = $"amqp://{factory.UserName}:{factory.Password}@{factory.HostName}";
            IisElasticConfiguration elasticConfiguration = Configuration.GetSection("elasticSearch").Get<IisElasticConfiguration>();

            services.AddHealthChecks()
                .AddNpgSql(dbConnectionString)
                .AddRabbitMQ(mqString, (SslOption)null)
                .AddElasticsearch(elasticConfiguration.Uri);

            var gsmWorkerUrl = Configuration.GetValue<string>("gsmWorkerUrl");
            services.AddSingleton<IGsmTranscriber>(e => new GsmTranscriber(gsmWorkerUrl));
            services.AddSingleton<IMaterialEventProducer, MaterialEventProducer>();

            services.AddSingleton<IElasticManager, IisElasticManager>();
            services.AddSingleton<IisElasticSerializer>();
            services.AddSingleton(elasticConfiguration);

            services.AddHostedService<MaterialEventConsumer>();

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
        }

        private void _authenticate(IQueryContext context, HashSet<string> publiclyAccesible)
        {
            // TODO: remove this method when hotchocolate will allow to add attribute for authentication
            var qd = context.Request.Query as QueryDocument;
            if (qd == null || qd.Document == null)
                throw new InvalidOperationException("Cannot find query in document");

            var odn = qd.Document.Definitions[0] as OperationDefinitionNode;
            if (odn.SelectionSet?.Selections.Count != 1)
                throw new InvalidOperationException("Does not support multiple selections in query");

            var fieldNode = (FieldNode)odn.SelectionSet.Selections[0];

            if (!publiclyAccesible.Contains(fieldNode.Name.Value))
            {
                var httpContext = (HttpContext)context.ContextData["HttpContext"];
                if (!httpContext.Request.Headers.TryGetValue("Authorization", out var token))
                    throw new AuthenticationException("Requires \"Authorization\" header to contain a token");

                var validatedToken = TokenHelper.ValidateToken(token, Configuration);
                context.ContextData.Add("token", validatedToken);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );

            app.UseSerilogRequestLogging();

            app.UseGraphQL();
            app.UsePlayground();
            app.UseHealthChecks("/api/server-health", new HealthCheckOptions { ResponseWriter = ReportHealthCheck });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static async Task ReportHealthCheck(HttpContext c, HealthReport r)
        {
            //c.Response.ContentType = MediaTypeNames.Application.Json;
            c.Response.ContentType = "application/health+json";
            var result = JsonConvert.SerializeObject(
                new
                {
                    version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
                    availability = r.Entries.Select(e =>
                        new {
                            description = e.Key,
                            status = e.Value.Status.ToString(),
                            responseTime = e.Value.Duration.TotalMilliseconds
                        }),
                    totalResponseTime = r.TotalDuration.TotalMilliseconds
                },
                Formatting.Indented);

            if (r.Entries.Any(x => x.Value.Status != HealthStatus.Healthy))
            {
                c.Response.StatusCode = 503;
            }

            await c.Response.WriteAsync(result);
        }
    }

    public class MqConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    class AppErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (error.Exception is InvalidOperationException || error.Exception is InvalidCredentialException)
            {

                return error.WithCode("BAD_REQUEST")
                    .WithMessage(error.Exception.Message);
            }

            if (error.Exception is AuthenticationException)
            {
                return error.WithCode("UNAUTHENTICATED");
            }

            return error;
        }
    }
}
