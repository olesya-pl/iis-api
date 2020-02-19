using AutoMapper;
using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager
{
    public class Startup
    {
        public void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<OntologyContext>(options => options
                .UseNpgsql("Server = localhost; Database = contour_prod; Username = postgres; Password = 123"));
            services.AddTransient<IOntologySchema, Iis.OntologySchema.OntologySchema>();
            services.AddTransient<MainForm>();
            services.AddSingleton<IOntologyManagerStyle>(OntologyManagerStyle.GetDefaultStyle());
            services.AddAutoMapper(typeof(Startup));
        }

    }
}
