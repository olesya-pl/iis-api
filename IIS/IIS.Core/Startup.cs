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
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.Tools;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

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
            services.RegisterRunUpTools();

            services.AddMemoryCache();

            var dbConnectionString = Configuration.GetConnectionString("db", "DB_");
            services.AddDbContext<OntologyContext>(options => options
                .UseNpgsql(dbConnectionString)
            // .EnableSensitiveDataLogging()
            );

            services.AddHttpContextAccessor();
            services.AddSingleton<IOntologyProvider, OntologyProvider>();
            services.AddTransient<ILegacyOntologyProvider, LegacyOntologyProvider>();
            services.AddTransient<ILegacyMigrator, LegacyMigrator>();
            services.AddTransient<IOntologyService, OntologyService>();
            services.AddTransient<OntologyTypeSaver>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IMaterialProvider, MaterialProvider>();
            services.AddTransient<IMaterialService, MaterialService>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

            // material processors
            services.AddTransient<IMaterialProcessor, Materials.EntityFramework.Workers.MetadataExtractor>();
            services.AddTransient<IMaterialProcessor, Materials.EntityFramework.Workers.Odysseus.PersonForm5Processor>();

            services.AddTransient<Ontology.Seeding.Seeder>();
            services.AddTransient(e => new ContextFactory(dbConnectionString));
            services.AddTransient(e => new FileServiceFactory(dbConnectionString));
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
            EsConfiguration es = Configuration.GetSection("es").Get<EsConfiguration>();
            services.AddHealthChecks()
                .AddNpgSql(dbConnectionString)
                .AddRabbitMQ(mqString, HealthStatus.Unhealthy);
                //.AddElasticsearch(es.Host);

            var gsmWorkerUrl = Configuration.GetValue<string>("gsmWorkerUrl");
            services.AddSingleton<IGsmTranscriber>(e => new GsmTranscriber(gsmWorkerUrl));
            services.AddSingleton<IMaterialEventProducer, MaterialEventProducer>();
            services.AddHostedService<MaterialEventConsumer>();

            //services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddControllers();
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, OntologyContext context)
        {
            app.UseSerilogRequestLogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<OptionsMiddleware>();
            app.UseGraphQL();
            app.UsePlayground();
            app.UseHealthChecks("/api/server-health", new HealthCheckOptions { ResponseWriter = ReportHealthCheck });

            //app.UseMvc();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SeedData(context);
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

        private void SeedData(OntologyContext context)
        {
            var defaultUserName = Configuration.GetValue<string>("defaultUserName");
            var defaultPassword = Configuration.GetValue<string>("defaultPassword");

            if (!string.IsNullOrWhiteSpace(defaultUserName) && !string.IsNullOrWhiteSpace(defaultPassword))
            {
                //var admin = context.Users.SingleOrDefault(u => u.Username.ToUpperInvariant() == defaultUserName.ToUpperInvariant());
                var admin = context.Users.SingleOrDefault(x => EF.Functions.Like(x.Name, $"%{defaultUserName}%"));
                if (admin == null)
                {
                    context.Users.Add(new Core.Users.EntityFramework.User
                    {
                        Id = Guid.NewGuid(),
                        IsBlocked = false,
                        Name = defaultUserName,
                        Username = defaultUserName,
                        PasswordHash = Configuration.GetPasswordHashAsBase64String(defaultPassword)
                    });
                    context.SaveChanges();
                }
            }
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
            context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)context.Request.Headers["Origin"] ?? string.Empty });
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

    public class EsConfiguration
    {
        public string Host { get; set; }
    }
}
