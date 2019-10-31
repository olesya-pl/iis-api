using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


            var publiclyAccesible = new HashSet<string> { "login", "__schema" };
            QueryExecutionBuilder.New()
                .Use(next => context => _authenticate(context, next, publiclyAccesible))
                .UseDefaultPipeline()
                .Populate(services);

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
                        ValidIssuer = Configuration.GetValue<string>("jwt:issuer"),

                        ValidateAudience         = true,
                        ValidAudience            = Configuration.GetValue<string>("jwt:audience"),
                        ValidateLifetime         = true,
                        IssuerSigningKey         = TokenHelper.GetSymmetricSecurityKey(Configuration.GetValue<string>("jwt:signingKey")),
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

        private Task _authenticate(IQueryContext context, QueryDelegate next, HashSet<string> publiclyAccesible)
        {
            // TODO: remove this method when hotchocolate will allow to add attribute for authentication
            Task Error(string message, string code)
            {
                var error = ErrorBuilder
                    .New()
                    .SetMessage(message)
                    .SetCode(code)
                    .Build();
                context.Result = QueryResult.CreateError(error);
                return Task.CompletedTask;
            }

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
                if (!httpContext.Request.Headers.TryGetValue("Authorization", out var value))
                {
                    return Error("You are not authenticated", "UNAUTHENTICATED");
                }

                var headerValueParts = value.ToString().Split(' ');
                var token = headerValueParts.Length == 1 ? headerValueParts[0] : headerValueParts[1];
                var (success, message) = TokenHelper.ValidateToken(token, new TokenValidationParameters
                {
                    ValidIssuer = Configuration.GetValue<string>("jwt:issuer"),
                    ValidAudience = Configuration.GetValue<string>("jwt:audience"),
                    IssuerSigningKey = TokenHelper.GetSymmetricSecurityKey(Configuration.GetValue<string>("jwt:signingKey")),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true
                });

                if (!success)
                    return Error(message, "UNAUTHENTICATED");
            }

            return next(context);
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
            app.UseAuthentication();
            app.UseMvc();

            SeedData(context);
        }

        private void SeedData(OntologyContext context)
        {
            var defaultUserName = Configuration.GetValue<string>("defaultUserName");
            var defaultPassword = Configuration.GetValue<string>("defaultPassword");

            if (!string.IsNullOrWhiteSpace(defaultUserName) && !string.IsNullOrWhiteSpace(defaultPassword))
            {
                var admin = context.Users.SingleOrDefault(u => u.Username.ToUpperInvariant() == defaultUserName.ToUpperInvariant());
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
