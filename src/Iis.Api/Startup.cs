using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Iis.Api;
using Iis.Api.Authentication.OntologyBasicAuthentication;
using Iis.Api.BackgroundServices;
using Iis.Api.Bootstrap;
using Iis.Api.Configuration;
using Iis.Api.EventHandlers;
using Iis.Api.Export;
using Iis.Api.FlightRadar;
using Iis.Api.GraphQL.Access;
using Iis.Api.Metrics;
using Iis.Api.Modules;
using Iis.Api.Ontology;
using Iis.CoordinatesEventHandler.DependencyInjection;
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Materials;
using IIS.Core.Materials.EntityFramework;
using IIS.Core.Materials.EntityFramework.FeatureProcessors;
using IIS.Core.Materials.FeatureProcessors;
using IIS.Core.Ontology.EntityFramework;
using Iis.DataModel;
using Iis.DataModel.Cache;
using Iis.DbLayer.Common;
using Iis.DbLayer.DirectQueries;
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
using Iis.FlightRadar.DataModel;
using Iis.Interfaces.Common;
using Iis.Interfaces.DirectQueries;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.SecurityLevels;
using Iis.OntologyData;
using Iis.RabbitMq.DependencyInjection;
using IIS.Repository.Factories;
using Iis.Security.SecurityLevels;
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
using Iis.Utility.Csv;
using Iis.Utility.Logging;
using Prometheus;
using IIS.Core.GraphQL;
using Iis.Api.Authentication.OntologyJwtBearerAuthentication;
using Iis.Api.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IIS.Core
{
    public class Startup
    {
        public ILoggerFactory MyLoggerFactory;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
#if DEBUG
            MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
#endif
        }

        public IConfiguration Configuration { get; }

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
            services.AddSingleton<ISecurityLevelChecker, SecurityLevelChecker>();
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

            services.AddGraphQLServer()
                .AddErrorFilter<AppErrorFilter>()
                .ConfigureSchema()
                .AddIdSerializer()
                .AddInMemorySubscriptions()
                .UseDefaultPipeline()
                .AddHttpRequestInterceptor<AuthenticationInterceptor>()
                .AddAuthorization();

            /* message queue registration*/
            services.RegisterMqFactory(Configuration, out _)
                    .RegisterMaterialEventServices(Configuration);

            services.RegisterElasticManager(Configuration, out ElasticConfiguration elasticConfiguration);

            var maxOperatorsConfig = Configuration.GetSection("maxMaterialsPerOperator").Get<MaxMaterialsPerOperatorConfig>();
            services.AddSingleton(maxOperatorsConfig);
            services.RegisterFlightRadarServices(Configuration);

            if (enableContext)
            {
                services.AddHealthChecks()
                .AddNpgSql(dbConnectionString)
                .AddRabbitMQ(_ => _.GetRequiredService<RabbitMQ.Client.IConnection>())
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

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddOntologyJwtBearerAuthentication(Configuration)
                .AddOntologyScheme();

            services.AddServiceAuthorization();

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup), typeof(CsvDataItem));
            services.AddTransient<GraphQLAccessList>();

            services.RegisterRepositories();
            services.AddMediatR(typeof(ReportEventHandler));
            services.AddTransient<ModifyDataRunner>();
            services.RegisterEventMaterialAutoAssignment(Configuration);
            services.RegisterCoordinatesMessageHandler(Configuration);

            var eusConfiguration = Configuration.GetSection("externalUserService").Get<ExternalUserServiceConfiguration>();
            services.AddSingleton(eusConfiguration);
            services.AddTransient<IExternalUserService, ActiveDirectoryUserService>();

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
                        .AllowAnyMethod());
            }

            app.UseMiddleware<LogHeaderMiddleware>();

#if !DEBUG
            app.UseMiddleware<LoggingMiddleware>();
#endif
            app.UseHealthChecks("/api/server-health", new HealthCheckOptions { ResponseWriter = ReportHealthCheck });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
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