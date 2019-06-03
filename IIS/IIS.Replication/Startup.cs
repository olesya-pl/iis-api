using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace IIS.Replication
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
            var mq = Configuration.GetSection("mq").Get<MqConfiguration>();
            var factory = new ConnectionFactory
            {
                HostName = mq.Host,
                UserName = mq.Username,
                Password = mq.Password,
                RequestedConnectionTimeout = 3 * 60 * 1000, // why this shit doesn't work
            };
            var es = Configuration.GetSection("es").Get<EsConfiguration>();
            services.AddTransient<IReplicationService>(s => new ReplicationService(es.Host));
            services.AddTransient(s => factory);
            services.AddHostedService<ReplicationHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Replicator");
            });
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
