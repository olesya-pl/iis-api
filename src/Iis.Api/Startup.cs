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
using Iis.DbLayer.Common;
using Iis.DbLayer.Elastic;
using Iis.DbLayer.ModifyDataScripts;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.DbLayer.OntologyData;
using Iis.DbLayer.OntologySchema;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.Vocabularies;
using Iis.Elastic;
using Iis.EventMaterialAutoAssignment;
using Iis.RabbitMq.DependencyInjection;
using Iis.FlightRadar.DataModel;
using Iis.Interfaces.Common;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Roles;
using Iis.OntologyData;
using Iis.Services;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Csv;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Matrix;
using Iis.Services.DI;
using Iis.Services.ExternalUserServices;
using Iis.Services.MatrixServices;
using Iis.Utility;
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.GraphQL;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials;
using IIS.Core.Materials.EntityFramework;
using IIS.Core.Materials.EntityFramework.FeatureProcessors;
using IIS.Core.Materials.FeatureProcessors;
using IIS.Core.Ontology.EntityFramework;
using IIS.Repository.Factories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
using Iis.CoordinatesEventHandler.DependencyInjection;
using Iis.Utility.Csv;
using Iis.Utility.Logging;
using Iis.Api.Authentication.OntologyBasicAuthentication;
using Iis.Interfaces.DirectQueries;
using Iis.DbLayer.DirectQueries;
using Prometheus;
using Iis.Api.Metrics;
using Iis.Interfaces.SecurityLevels;
using Iis.Security.SecurityLevels;

namespace IIS.Core
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public ILoggerFactory MyLoggerFactory;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
#if DEBUG
            MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
#endif
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
                .AddConfigurations(Configuration);

            services.AddMemoryCache();

            var connectionStringService = new ConnectionStringService(Configuration);
            var dbConnectionString = connectionStringService.GetIisApiConnectionString();
            var flightRadarDbConnectionString = connectionStringService.GetFlightRadarConnectionString();
            services.AddTransient(provider => new DbContextOptionsBuilder().UseNpgsql(dbConnectionString).Options);
            if (enableContext)
            {
                services.AddDbContext<OntologyContext>(
                    options => options
                        .UseNpgsql(dbConnectionString)
                        .UseLoggerFactory(MyLoggerFactory),
                    contextLifetime: ServiceLifetime.Transient,
                    optionsLifetime: ServiceLifetime.Transient);

                services.AddDbContext<AssignmentConfigContext>(
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
                services.AddSingleton<IOntologySchema>(provider => new OntologySchemaService().GetOntologySchema(schemaSource));
                services.AddSingleton<ICommonData>(provider =>
                {
                    var ontologyData = provider.GetRequiredService<IOntologyNodesData>();
                    return new CommonData(ontologyData);
                });
                services.AddSingleton<ISecurityLevelChecker, SecurityLevelChecker>();

                services.AddTransient<IFieldToAliasMapper>(provider => provider.GetRequiredService<IOntologySchema>());
                services.AddTransient<INodeSaveService, NodeSaveService>();
                services.AddTransient<ElasticConfiguration>();
            }

            services.AddHttpContextAccessor();

            services.AddTransient<IUnitOfWorkFactory<IIISUnitOfWork>, IISUnitOfWorkFactory>();
            services.AddTransient<IMaterialService, MaterialService<IIISUnitOfWork>>();
            services.AddTransient<IOntologyService, OntologyServiceWithCache>();

            services.AddSingleton<IElasticConfiguration, IisElasticConfiguration>();
            services.AddTransient<MutationCreateResolver>();
            services.AddTransient<IOntologySchemaSource, OntologySchemaSource>();
            services.AddTransient<MutationUpdateResolver>();
            services.AddTransient<MutationDeleteResolver>();
            services.AddTransient<IExtNodeService, ExtNodeService>();
            services.AddTransient<IFileService, FileService<IIISUnitOfWork>>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
            services.AddTransient<IElasticService, ElasticService>();
            services.AddTransient<IGroupedAggregationNameGenerator, GroupedAggregationNameGenerator>();
            services.AddTransient<OntologySchemaService>();
            services.AddTransient<ExportService>();
            services.AddTransient<ExportToJsonService>();
            services.AddTransient<RoleService>();
            services.AddTransient<IUserService, UserService<IIISUnitOfWork>>();
            services.AddTransient<IThemeService, ThemeService<IIISUnitOfWork>>();
            services.AddTransient<IOntologyDataService, OntologyDataService>();
            services.AddTransient<IAnnotationsService, AnnotationsService>();
            services.AddTransient<IOntologySchemaService, OntologySchemaService>();
            services.AddTransient<IConnectionStringService, ConnectionStringService>();
            services.AddTransient<IAccessLevelService, AccessLevelService>();
            services.AddTransient<ISecurityLevelService, SecurityLevelService>();
            services.AddTransient<AccessObjectService>();
            services.AddTransient<IFeatureProcessorFactory, FeatureProcessorFactory>();
            services.AddTransient<NodeMapper>();
            services.AddTransient<FileUrlGetter>();
            services.AddTransient<PropertyTranslator>();
            services.AddTransient<ICsvService, CsvService>();

            services.AddTransient<IChangeHistoryService, ChangeHistoryService<IIISUnitOfWork>>();
            services.AddTransient<ILocationHistoryService, LocationHistoryService<IIISUnitOfWork>>();
            services.AddSingleton<GraphQL.ISchemaProvider, GraphQL.SchemaProvider>();
            services.AddTransient<GraphQL.Entities.IOntologyFieldPopulator, GraphQL.Entities.OntologyFieldPopulator>();
            services.AddTransient<GraphQL.Entities.Resolvers.IOntologyMutationResolver, GraphQL.Entities.Resolvers.OntologyMutationResolver>();
            services.AddTransient<GraphQL.Entities.Resolvers.IOntologyQueryResolver, GraphQL.Entities.Resolvers.OntologyQueryResolver>();
            services.AddSingleton<GraphQL.Entities.TypeRepository>(); // For HotChocolate ontology types creation. Should have same lifetime as GraphQL schema
            // Here it hits the fan. Removed AddGraphQL() method and stripped it to submethods because of IncludeExceptionDetails.
            // todo: remake graphql engine registration in DI
            //services.AddGraphQL(schema);

            var publiclyAccesible = new HashSet<string> { "login", "__schema" };

            QueryExecutionBuilder.New()
                .Use(next => async context =>
                {
                    try
                    {
                        await AuthenticateAsync(context, publiclyAccesible);
                    }
                    catch (Exception e)
                    {
                        if (!(e is AuthenticationException) && !(e is InvalidOperationException) && !(e is AccessViolationException))
                            throw;

                        var errorHandler = context.Services.GetService<IErrorHandler>();
                        var error = ErrorBuilder.New()
                            .SetMessage(e.Message)
                            .SetException(e)
                            .Build();
                        context.Exception = e;
                        context.Result = QueryResult.CreateError(errorHandler.Handle(error));
                        return;
                    }

                    await next(context);
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

            services.RegisterElasticManager(Configuration, out ElasticConfiguration elasticConfiguration);

            var maxOperatorsConfig = Configuration.GetSection("maxMaterialsPerOperator").Get<MaxMaterialsPerOperatorConfig>();
            services.AddSingleton(maxOperatorsConfig);
            services.RegisterFlightRadarServices(Configuration);

            if (enableContext)
            {
                services.AddHealthChecks()
                .AddNpgSql(dbConnectionString)
                .AddRabbitMQ(mqConnectionString, (SslOption)null)
                .ForwardToPrometheus()
                .AddElasticsearch(options => options
                    .UseServer(elasticConfiguration.Uri)
                    .UseBasicAuthentication(elasticConfiguration.DefaultLogin, elasticConfiguration.DefaultPassword));
            }

            services.AddTransient<IElasticSerializer, ElasticSerializer>();
            services.AddTransient<IIisElasticConfigService, IisElasticConfigService<IIISUnitOfWork>>();

            services.AddTransient<IAutocompleteService, AutocompleteService>();
            services.AddTransient<IReportService, ReportService<IIISUnitOfWork>>();
            services.AddTransient<IReportElasticService, ReportElasticService>();
            services.AddTransient<IActiveDirectoryClient, ActiveDirectoryClient>(_ =>
                new ActiveDirectoryClient(
                    Configuration["activeDirectory:server"],
                    Configuration["activeDirectory:login"],
                    Configuration["activeDirectory:password"]));
            services.AddSingleton<IElasticState, ElasticState>();
            services.AddTransient<IAdminOntologyElasticService, AdminOntologyElasticService>();
            services.AddHostedService<ThemeCounterBackgroundService>();
            services.AddServices();

            services.AddAuthentication()
                .AddOntologyScheme();
            services.AddAuthorization();

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup), typeof(CsvDataItem));
            services.AddTransient<GraphQLAccessList>();

            services.RegisterRepositories();
            services.AddMediatR(typeof(ReportEventHandler));
            services.AddTransient<ModifyDataRunner>();
            services.RegisterEventMaterialAutoAssignment(Configuration);
            services.RegisterCoordinatesMessageHandler(Configuration);

            var eusConfiguration = Configuration.GetSection("externalUserService").Get<ExternalUserServiceConfiguration>();
            services.AddTransient<IExternalUserService>(_ => new ExternalUserServiceFactory().GetInstance(eusConfiguration));

            var matrixConfiguration = Configuration.GetSection("matrix").Get<MatrixServiceConfiguration>();
            services.AddSingleton(matrixConfiguration);
            services.AddTransient<IMatrixService, MatrixService>();

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = long.MaxValue;
            });

            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = long.MaxValue);
            services.AddSingleton<IDirectQueryFactory>(new DirectQueryFactory(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)));

            services.AddMetrics();
        }

        private async Task AuthenticateAsync(IQueryContext context, HashSet<string> publiclyAccesible)
        {
            // TODO: remove this method when hotchocolate will allow to add attribute for authentication
            var qd = context.Request.Query as QueryDocument;
            if (qd == null || qd.Document == null)
            {
                throw new InvalidOperationException("Cannot find query in document");
            }

            var odn = qd.Document.Definitions[0] as OperationDefinitionNode;
            if (odn.SelectionSet?.Selections.Count != 1)
            {
                throw new InvalidOperationException("Does not support multiple selections in query");
            }

            var fieldNode = (FieldNode)odn.SelectionSet.Selections[0];

            if (!publiclyAccesible.Contains(fieldNode.Name.Value))
            {
                var httpContext = (HttpContext)context.ContextData["HttpContext"];
                if (!httpContext.Request.Headers.TryGetValue("Authorization", out var token))
                {
                    throw new AuthenticationException("Requires \"Authorization\" header to contain a token");
                }

                var userService = context.Services.GetService<IUserService>();
                var graphQLAccessList = context.Services.GetService<GraphQLAccessList>();

                var operationName = context.Request.OperationName ?? fieldNode.Name.Value;

                var graphQLAccessItems = graphQLAccessList.GetAccessItem(operationName, context.Request.VariableValues);

                var validatedToken = await TokenHelper.ValidateTokenAsync(token, Configuration, userService);

                foreach (var graphQLAccessItem in graphQLAccessItems)
                {
                    if (graphQLAccessItem == null || graphQLAccessItem.Kind == AccessKind.FreeForAll)
                    {
                        break;
                    }
                    if (!validatedToken.User.IsGranted(graphQLAccessItem.Kind, graphQLAccessItem.Operation, AccessCategory.Entity))
                    {
                        throw new AccessViolationException($"Access denied to {operationName} for user {validatedToken.User.UserName}");
                    }
                }

                context.ContextData.Add(TokenPayload.TokenPropertyName, validatedToken);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
            LoadHotChockolateSchema(app);
            app.UseHealthChecks("/api/server-health", new HealthCheckOptions { ResponseWriter = ReportHealthCheck });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }

        private void LoadHotChockolateSchema(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();
            var schemaProvider = serviceScope.ServiceProvider.GetRequiredService<ISchemaProvider>();
            schemaProvider.GetSchema();
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