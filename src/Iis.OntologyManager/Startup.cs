using AutoMapper;
using Iis.DataModel;
using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologyManager.UiControls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Iis.OntologyManager
{
    public class Startup
    {
        public void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<IOntologySchema, Iis.OntologySchema.OntologySchema>();
            services.AddTransient<MainForm>();
            services.AddTransient<UiControlsCreator>();
            services.AddTransient<OntologySchemaService>();
            services.AddSingleton<IOntologyManagerStyle>(OntologyManagerStyle.GetDefaultStyle());
            services.AddAutoMapper(typeof(Startup));
        }

    }
}
