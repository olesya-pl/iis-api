using AutoMapper;
using Iis.DataModel;
using Iis.DbLayer.Repositories;
using Iis.MaterialLoader.Rabbit;
using Iis.MaterialLoader.Services;
using IIS.Repository.Factories;
using Iis.RabbitMq.DependencyInjection;
using Iis.Services;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Interfaces;
using Iis.Utility.Csv;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IChangeHistoryService = Iis.MaterialLoader.Services.IChangeHistoryService;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Iis.Utility.Logging;

namespace Iis.MaterialLoader
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
            services.AddControllers();
            services.AddHealthChecks();

            var dbConnectionString = Configuration.GetConnectionString("db");

            services.AddDbContext<OntologyContext>(
                options => options
                    .UseNpgsql(dbConnectionString),
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Transient);
            services.AddTransient<IChangeHistoryRepository, ChangeHistoryRepository>();
            services.AddTransient<IUnitOfWorkFactory<IIISUnitOfWork>, IISUnitOfWorkFactory>();
            services.AddTransient<IMaterialService, MaterialService>();
            services.AddTransient<IChangeHistoryService, ChangeHistoryService>();

            services.AddAutoMapper(typeof(Startup), typeof(CsvDataItem));
            services.AddScoped<IFileService, FileService<IIISUnitOfWork>>();
            services.AddTransient<IFileRepository, FileRepository>();
            services.AddSingleton(Configuration.GetSection("files").Get<FilesConfiguration>());
            services.RegisterMqFactory(Configuration, out _);
            services.RegisterMqInstances(Configuration);

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = long.MaxValue;
            });

            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = long.MaxValue);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<LogHeaderMiddleware>();

#if !DEBUG
            app.UseMiddleware<LoggingMiddleware>();
#endif
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}