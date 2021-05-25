using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iis.DataModel;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Iis.Api
{
    public static class ApplicationBuilderExtensions
    {
        private class MilitaryAmountEntry
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        private class MartialStatusEntry
        {
            public string Name { get; set; }
        }

        public static void UpdateMartialStatus(this IApplicationBuilder app)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
                {
                    if (!File.Exists("data/contour/entities/MartialStatus.json")) return;

                    var text = File.ReadAllText("data/contour/entities/MartialStatus.json");
                    var martialStatuses = JsonConvert.DeserializeObject<List<MartialStatusEntry>>(text);

                    var serviceProvider = serviceScope.ServiceProvider;
                    var ontologyData = serviceProvider.GetRequiredService<IOntologyNodesData>();

                    var martialStatusType = ontologyData.Schema.GetEntityTypeByName("MartialStatus");
                    var nameRelation = martialStatusType.GetRelationTypeByName("name");

                    foreach (var entry in martialStatuses)
                    {
                        if (ontologyData.Attributes.Any(p => p.Value == entry.Name 
                            && p.Node.IncomingRelations.Any(p => p.Node?.NodeTypeId == nameRelation.NodeType.Id)))
                        {
                            continue;
                        }
                        var node = ontologyData.CreateNode(martialStatusType.Id);
                        ontologyData.CreateRelationWithAttribute(node.Id, nameRelation.NodeType.Id, entry.Name);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

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

        public static void SeedExternalUsers(this IApplicationBuilder host)
        {
            using IServiceScope scope = host.ApplicationServices.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            userService.ImportUsersFromExternalSource();
        }
    }
}
