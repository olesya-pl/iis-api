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
using Iis.Elastic;
using Iis.Api;
using Iis.Api.Modules;
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
using Iis.DataModel.Cache;
using Iis.DbLayer.OntologySchema;
using Iis.DataModel.Roles;
using Iis.Roles;
using Iis.Api.GraphQL.Access;
using Iis.Interfaces.Roles;
using IIS.Core.ML;
using IIS.Core.NodeMaterialRelation;

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
            RegisterServices(services, true);
        }

        public void RegisterServices(IServiceCollection services, bool enableContext)
        {
            services
                .RegisterRunUpTools()
                .RegisterSeederTools()
                .AddConfigurations(Configuration);

            services.AddMemoryCache();

            var dbConnectionString = Configuration.GetConnectionString("db", "DB_");

            if (enableContext)
            {
                services.AddDbContext<OntologyContext>(
                    options => options.UseNpgsql(dbConnectionString),
                    ServiceLifetime.Transient);
                using var context = OntologyContext.GetContext(dbConnectionString);
                context.Database.Migrate();
                (new FillDataForRoles(context)).Execute();
                services.AddSingleton<IOntologyCache>(new OntologyCache(context));
            }

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
            services.AddTransient<OntologySchemaService>();
            services.AddSingleton<RunTimeSettings>();
            services.AddScoped<ExportService>();
            services.AddScoped<ExportToJsonService>();
            services.AddTransient<RoleService>();
            services.AddTransient<AccessObjectService>();
            services.AddTransient<MlProcessingService>();
            services.AddTransient<NodeMaterialRelationService>();

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
                        _authenticate(context, publiclyAccesible);
                    }
                    catch (Exception e)
                    {
                        if (!(e is AuthenticationException) && !(e is InvalidOperationException) && !(e is AccessViolationException))
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

            /* message queue registration*/
            services.RegisterMqFactory(Configuration, out string mqConnectionString)
                    .RegisterMaterialEventServices(Configuration);


            ElasticConfiguration elasticConfiguration = Configuration.GetSection("elasticSearch").Get<ElasticConfiguration>();

            services.AddHealthChecks()
                .AddNpgSql(dbConnectionString)
                .AddRabbitMQ(mqConnectionString, (SslOption)null)
                .AddElasticsearch(elasticConfiguration.Uri);


            services.AddSingleton<IElasticManager, ElasticManager>();
            services.AddSingleton<IElasticSerializer, ElasticSerializer>();
            services.AddSingleton(elasticConfiguration);

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddSingleton<GraphQLAccessList>();
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

                var roleLoader = context.Services.GetService<RoleService>();
                var graphQLAccessList = context.Services.GetService<GraphQLAccessList>();

                var graphQLAccessItem = graphQLAccessList.GetAccessItem(context.Request.OperationName ?? fieldNode.Name.Value);
                var validatedToken = TokenHelper.ValidateToken(token, Configuration, roleLoader);

                if (graphQLAccessItem != null && graphQLAccessItem.Kind != AccessKind.FreeForAll)
                {
                    if (!validatedToken.User.IsGranted(graphQLAccessItem.Kind, graphQLAccessItem.Operation))
                    {
                        throw new AccessViolationException($"Access denied to {context.Request.OperationName} for user {validatedToken.User.Username}");
                    }
                }

                context.ContextData.Add("token", validatedToken);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            UpdateDatabase(app);

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

        private void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<OntologyContext>())
                {
                    context.Database.Migrate();
                }
            }
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

            if (error.Exception is AccessViolationException)
            {
                return error.WithCode("ACCESS_DENIED");
            }

            return error;
        }
    }
}
