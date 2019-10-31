using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Interceptors;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.Configuration;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
            //services.AddGraphQL(schema);


            var anonimAcccess = new HashSet<string> { "login", "IntrospectionQuery" };
            QueryExecutionBuilder.New()
                .Use(next => context =>
                {
                    if (!Configuration.GetValue<bool>("useAuthentication"))
                    {
                        return next(context);
                    }

                    Task Error(string error)
                    {
                        var errorBuilder = new ErrorBuilder();
                        var errorObject = errorBuilder.SetMessage(error).Build();
                        context.Result =  QueryResult.CreateError(errorObject);;
                        return Task.CompletedTask;
                    }

                    var qd = context.Request.Query as QueryDocument;
                    if (qd == null)
                        return Error("Internal server error. context.Request.Query is not QueryDocument");
                    if (qd.Document == null)
                        return Error("Internal server error. context.Request.Query.Document is null");
                    if (qd.Document.Definitions.Count != 1)
                        return Error("Internal server error. Document.Definitions.Count must be 1");

                    var odn = qd.Document.Definitions[0] as OperationDefinitionNode;
                    if (odn.SelectionSet?.Selections.Count != 1)
                        return Error("Internal server error. SelectionSet?.Selections.Count != 1");

                    if (!(odn.SelectionSet.Selections[0] is FieldNode fn))
                        return Error("Internal server error. Selections[0] is not FieldNode ");

                    if (!anonimAcccess.Contains(fn.Name.Value))
                    {
                        var httpContext = (HttpContext)context.ContextData["HttpContext"];
                        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var value))
                        {
                            return Error("User not authorized");
                        }

                        var headerValueParts = value.ToString().Split(' ');
                        var token = headerValueParts.Length == 1 ? headerValueParts[0] : headerValueParts[1];
                        var (success, message) = TokenHelper.ValidateToken(token, new TokenValidationParameters
                        {
                            ValidIssuer = Configuration.GetValue<string>("ValidIssuer"),
                            ValidAudience = Configuration.GetValue<string>("ValidateAudience"),
                            IssuerSigningKey = TokenHelper.GetSymmetricSecurityKey(Configuration.GetValue<string>("IssuerSigningKey")),
                            ValidateIssuerSigningKey = true,
                            ValidateAudience = true
                        });

                        if (!success)
                            return Error(message);
                    }

                    return next(context);
                })
                .UseDefaultPipeline().Populate(services);

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

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Configuration.GetValue<string>("ValidIssuer"),

                        ValidateAudience         = true,
                        ValidAudience            = Configuration.GetValue<string>("ValidateAudience"),
                        ValidateLifetime         = true,
                        IssuerSigningKey         = TokenHelper.GetSymmetricSecurityKey(Configuration.GetValue<string>("IssuerSigningKey")),
                        ValidateIssuerSigningKey = true,
                    };
                });

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                var filter = new AuthorizeFilter(policy);
                config.Filters.Add(filter);
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, OntologyContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<OptionsMiddleware>();
            app.UseGraphQL();
            app.UsePlayground();

            if (Configuration.GetValue<bool>("useAuthentication"))
            {
                app.UseAuthentication();
            }

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
