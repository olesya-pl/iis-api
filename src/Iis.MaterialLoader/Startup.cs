using AutoMapper;
using Iis.DataModel;
using Iis.DbLayer.Repositories;
using IIS.Repository.Factories;
using Iis.Services;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            
            var dbConnectionString = Configuration.GetConnectionString("db");
            // services.AddDbContextPool<OntologyContext>(options => options.UseNpgsql(dbConnectionString));

            services.AddDbContext<OntologyContext>(
                options => options
                    .UseNpgsql(dbConnectionString),
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Transient);
            services.AddTransient<IChangeHistoryRepository, ChangeHistoryRepository>();
            services.AddTransient<IUnitOfWorkFactory<IIISUnitOfWork>, IISUnitOfWorkFactory>();
            services.AddTransient<IMaterialService, MaterialService>();
            services.AddTransient<IChangeHistoryService, ChangeHistoryService>();

            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IFileService, FileService<IIISUnitOfWork>>();
            services.AddTransient<IFileRepository, FileRepository>();
            services.AddSingleton(Configuration.GetSection("files").Get<FilesConfiguration>());
            services.AddSingleton<IMaterialEventProducer, MaterialEventProducer>();
            services.AddRabbit(Configuration, out _);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors(builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
