using AutoMapper;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.Execution;
using HotChocolate.Execution.Batching;
using HotChocolate.Execution.Configuration;
using HotChocolate.Language;
using HotChocolate.Types.Relay;
using Iis.Api;
using Iis.Api.BackgroundServices;
using Iis.Api.Bootstrap;
using Iis.Api.Configuration;
using Iis.Api.EventHandlers;
using Iis.Api.Export;
using Iis.Api.FlightRadar;
using Iis.Api.GraphQL.Access;
using Iis.Api.Modules;
using Iis.Api.Ontology;
using Iis.DataModel;
using Iis.DataModel.Cache;
using Iis.DbLayer.Elastic;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.DbLayer.OntologyData;
using Iis.DbLayer.OntologySchema;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.Vocabularies;
using Iis.Elastic;
using Iis.FlightRadar.DataModel;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Roles;
using Iis.OntologyData;
using Iis.Services;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.DI;
using Iis.Utility;
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials;
using IIS.Core.Materials.EntityFramework;
using IIS.Core.Materials.EntityFramework.FeatureProcessors;
using IIS.Core.Materials.FeatureProcessors;
using IIS.Core.NodeMaterialRelation;
using IIS.Core.Ontology.EntityFramework;
using IIS.Core.Tools;
using IIS.Repository.Factories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace IIS.Core
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

#if DEBUG
        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
#endif

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
            services.AddMvc().AddNewtonsoftJson(x =>
            {
                x.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            services
                .RegisterRunUpTools()
                .RegisterSeederTools()
                .AddConfigurations(Configuration);

            services.AddMemoryCache();

            var dbConnectionString = Configuration.GetConnectionString("db", "DB_");
            var flightRadarDbConnectionString = Configuration.GetConnectionString("db-flightradar", "DB_");
            services.AddTransient(provider => new DbContextOptionsBuilder().UseNpgsql(dbConnectionString).Options);
            if (enableContext)
            {
#if DEBUG
                services.AddDbContext<OntologyContext>(
                    options => options
                        .UseNpgsql(dbConnectionString)
                        .UseLoggerFactory(MyLoggerFactory),
                    contextLifetime: ServiceLifetime.Transient,
                    optionsLifetime: ServiceLifetime.Transient);

                services.AddDbContext<FlightsContext>(
                    options => options
                        .UseNpgsql(flightRadarDbConnectionString)
                        .UseLoggerFactory(MyLoggerFactory)
                        .AddInterceptors(new FlightsContextInterceptor()),
                    contextLifetime: ServiceLifetime.Transient,
                    optionsLifetime: ServiceLifetime.Transient);
#else
                services.AddDbContext<OntologyContext>(
                                    options => options
                                        .UseNpgsql(dbConnectionString),
                                    contextLifetime: ServiceLifetime.Transient,
                                    optionsLifetime: ServiceLifetime.Transient);

                services.AddDbContext<FlightsContext>(
                                    options => options
                                        .UseNpgsql(flightRadarDbConnectionString)
                                        .AddInterceptors(new FlightsContextInterceptor()),
                                    contextLifetime: ServiceLifetime.Transient,
                                    optionsLifetime: ServiceLifetime.Transient);
#endif

                var schemaSource = new OntologySchemaSource
                {
                    Title = "DB",
                    SourceKind = SchemaSourceKind.Database,
                    Data = dbConnectionString
                };
                services.AddTransient<IOntologyPatchSaver, OntologyPatchSaver>();
                services.AddSingleton<IOntologyNodesData, OntologyNodesData>(provider => 
                {
                    using var context = OntologyContext.GetContext(dbConnectionString);

                    var ontologySaver = new OntologyPatchSaver(OntologyContext.GetContext(dbConnectionString));
                    var rawData = new NodesRawData(context.Nodes, context.Relations, context.Attributes);
                    var ontologySchema = provider.GetRequiredService<IOntologySchema>();

                    return new OntologyNodesData(rawData, ontologySchema, ontologySaver);
                });
                services.AddSingleton<IOntologyCache, OntologyCache>();
                services.AddSingleton<IOntologySchema>(provider => (new OntologySchemaService()).GetOntologySchema(schemaSource));
                services.AddTransient<IFieldToAliasMapper>(provider => provider.GetRequiredService<IOntologySchema>());
                services.AddTransient<INodeRepository, NodeRepository>();
                services.AddTransient<ElasticConfiguration>();
            }

            services.AddHttpContextAccessor();

            services.AddTransient<IUnitOfWorkFactory<IIISUnitOfWork>, IISUnitOfWorkFactory>();
            services.AddTransient<IMaterialService, MaterialService<IIISUnitOfWork>>();
            services.AddTransient<IOntologyService, OntologyServiceWithCache>();
            services.AddTransient<IMaterialProvider, MaterialProvider<IIISUnitOfWork>>();
            services.AddHttpClient<MaterialProvider<IIISUnitOfWork>>();

            services.AddSingleton<IElasticConfiguration, IisElasticConfiguration>();
            services.AddTransient<MutationCreateResolver>();
            services.AddTransient<IOntologySchemaSource, OntologySchemaSource>();
            services.AddTransient<MutationUpdateResolver>();
            services.AddTransient<MutationDeleteResolver>();
            services.AddTransient<IExtNodeService, ExtNodeService>();
            services.AddTransient<IFileService, FileService<IIISUnitOfWork>>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
            services.AddTransient<IElasticService, ElasticService>();
            services.AddTransient<OntologySchemaService>();
            services.AddTransient<ExportService>();
            services.AddTransient<ExportToJsonService>();
            services.AddTransient<RoleService>();
            services.AddTransient<UserService>();
            services.AddTransient<IThemeService, ThemeService<IIISUnitOfWork>>();
            services.AddTransient<IAnnotationsService, AnnotationsService>();
            services.AddTransient<AccessObjectService>();
            services.AddTransient<NodeMaterialRelationService<IIISUnitOfWork>>();
            services.AddTransient<IFeatureProcessorFactory, FeatureProcessorFactory>();
            services.AddTransient<NodeToJObjectMapper>();
            services.AddSingleton<FileUrlGetter>();
            services.AddSingleton<IisVocabulary>();

            services.AddTransient<IChangeHistoryService, ChangeHistoryService<IIISUnitOfWork>>();
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
                .AddTransient<IBatchQueryExecutor, BatchQueryExecutor>()
                .AddTransient<IIdSerializer, IdSerializer>()
                .AddJsonQueryResultSerializer()
                .AddJsonArrayResponseStreamSerializer()
                .AddGraphQLSubscriptions();
            // end of graphql engine registration
            services.AddDataLoaderRegistry();

            /* message queue registration*/
            services.RegisterMqFactory(Configuration, out string mqConnectionString)
                    .RegisterMaterialEventServices(Configuration);


            ElasticConfiguration elasticConfiguration = Configuration.GetSection("elasticSearch").Get<ElasticConfiguration>();
            var maxOperatorsConfig = Configuration.GetSection("maxMaterialsPerOperator").Get<MaxMaterialsPerOperatorConfig>();
            services.AddSingleton(maxOperatorsConfig);

            services.RegisterFlightRadarServices(Configuration);


            if (enableContext)
            {
                services.AddHealthChecks()
                .AddNpgSql(dbConnectionString)
                .AddRabbitMQ(mqConnectionString, (SslOption)null)
                .AddElasticsearch(elasticConfiguration.Uri);
            }

            services.AddTransient<IElasticSerializer, ElasticSerializer>();
            services.AddSingleton(elasticConfiguration);
            services.AddTransient<IIisElasticConfigService, IisElasticConfigService<IIISUnitOfWork>>();

            services.AddTransient<IAutocompleteService, AutocompleteService>();
            services.AddTransient<IReportService, ReportService<IIISUnitOfWork>>();
            services.AddTransient<IReportElasticService, ReportElasticService>();
            services.AddTransient<ISanitizeService, SanitizeService>();
            services.AddTransient<IActiveDirectoryClient, ActiveDirectoryClient>(_ =>
                new ActiveDirectoryClient(
                    Configuration["activeDirectory:server"],
                    Configuration["activeDirectory:login"],
                    Configuration["activeDirectory:password"]));
            services.AddSingleton<IElasticState, ElasticState>();
            services.AddSingleton<IAdminOntologyElasticService, AdminOntologyElasticService>();
            services.AddHostedService<ThemeCounterBackgroundService>();
            services.AddServices();

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddTransient<GraphQLAccessList>();

            services.RegisterRepositories();
            services.RegisterElasticModules();
            services.AddMediatR(typeof(ReportEventHandler));
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

                var userService = context.Services.GetService<UserService>();
                var graphQLAccessList = context.Services.GetService<GraphQLAccessList>();

                var graphQLAccessItem = graphQLAccessList.GetAccessItem(context.Request.OperationName ?? fieldNode.Name.Value);

                var validatedToken = TokenHelper.ValidateToken(token, Configuration, userService);

                if (graphQLAccessItem != null && graphQLAccessItem.Kind != AccessKind.FreeForAll)
                {
                    if (!validatedToken.User.IsGranted(graphQLAccessItem.Kind, graphQLAccessItem.Operation))
                    {
                        throw new AccessViolationException($"Access denied to {context.Request.OperationName} for user {validatedToken.User.UserName}");
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
            app.UpdateMilitaryAmmountCodes();
            app.UpdateMartialStatus();
            app.ReloadElasticFieldsConfiguration();

            if (!Configuration.GetValue<bool>("disableCORS", false))
            {
                app.UseCors(builder =>
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                );
            }

            app.UseMiddleware<LogHeaderMiddleware>();

#if !DEBUG
            app.UseMiddleware<LoggingMiddleware>();
#endif
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
                    var defaultCommandTimeout = context.Database.GetCommandTimeout();
                    context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
                    context.Database.Migrate();
                    context.Database.SetCommandTimeout(defaultCommandTimeout);
                }
                try
                {
                    using (var context = serviceScope.ServiceProvider.GetService<FlightsContext>())
                    {
                        context.Database.Migrate();
                    }
                }
                catch { }
            }
        }

        private static async Task ReportHealthCheck(HttpContext c, HealthReport r)
        {
            c.Response.ContentType = "application/health+json";
            var result = JsonConvert.SerializeObject(
                new
                {
                    version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
                    availability = r.Entries.Select(e =>
                        new
                        {
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
}
