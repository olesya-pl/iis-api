using System;
using System.Collections.Generic;
using GraphiQl;
using GraphQL.DataLoader;
using GraphQL.Resolvers;
using IIS.Search.GraphQL;
using IIS.Search.Ontology;
using IIS.Search.Replication;
using IIS.Search.Resolving;
using IIS.Search.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using RabbitMQ.Client;

namespace IIS.Search
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mq = Configuration.GetSection("mq").Get<MqConfiguration>();
            var factory = new ConnectionFactory
            {
                HostName = mq.Host,
                UserName = mq.Username,
                Password = mq.Password,
                RequestedConnectionTimeout = 3 * 60 * 1000, // todo: why this shit doesn't work
            };

            services.AddTransient(s => factory);
            services.AddHostedService<ReplicationHostedService>();
            services.AddTransient<ISchemaProvider, SchemaRepository>();
            services.AddTransient<ISchemaRepository, SchemaRepository>();
            services.AddTransient<ISearchService, SearchService>();
            services.AddTransient<IGraphQLSchemaProvider, GraphQLSchemaProvider>();
            services.AddSingleton<IDataLoaderContextAccessor, DataLoaderContextAccessor>();
            services.AddSingleton<DataLoaderDocumentListener>();
            services.AddSingleton<IDictionary<string, IFieldResolver>>(s => new Dictionary<string, IFieldResolver>
            {
                ["relationInfo"] = null,
                ["attribute"] = new GenericAsyncResolver(new AttributeResolver()),
                ["entities"] = new GenericAsyncResolver(new EntitiesResolver(s.GetRequiredService<ISearchService>())),
                ["entityRelation"] = new GenericAsyncResolver(new EntityRelationResolver()),
            });

            var es = Configuration.GetSection("es").Get<EsConfiguration>();
            var node = new Uri(es.Host);
            var settings = new ConnectionSettings(node).ThrowExceptions();
            services.AddTransient<IElasticClient>(s => new ElasticClient(settings));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDeveloperExceptionPage();

            app.UseGraphiQl("/graphiql", "/api/graph");

            app.UseMvc();
        }
    }

    public class MqConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class EsConfiguration
    {
        public string Host { get; set; }
    }
}
