using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iis.DataModel;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Iis.Api
{
    public static class ApplicationBuilderExtensions
    {
        public static void ReloadElasticFieldsConfiguration(this IApplicationBuilder app)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
                {
                    var serviceProvider = serviceScope.ServiceProvider;
                    var elasticConfig = serviceProvider.GetRequiredService<IElasticConfiguration>();
                    var context = serviceProvider.GetRequiredService<OntologyContext>();
                    elasticConfig.ReloadFields(context.ElasticFields.AsNoTracking().ToList());
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}