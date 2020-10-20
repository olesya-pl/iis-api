using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iis.DataModel;
using Iis.Domain;
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
        private class MilitaryAmountEntry
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public static void UpdateMilitaryAmmountCodes(this IApplicationBuilder app)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
                {
                    var serviceProvider = serviceScope.ServiceProvider;
                    var ontologyData = serviceProvider.GetRequiredService<IOntologyNodesData>();

                    var amountType = ontologyData.Schema.GetEntityTypeByName("MilitaryAmount");
                    var codeRelationType = amountType.GetRelationTypeByName("code");
                    if (amountType == null || codeRelationType == null) return;
                    if (!File.Exists("data/contour/entities/MilitaryAmount.json")) return;

                    var text = File.ReadAllText("data/contour/entities/MilitaryAmount.json");
                    var jsonMilitaryAmounts = JsonConvert.DeserializeObject<List<MilitaryAmountEntry>>(text);

                    foreach (var militaryAmount in jsonMilitaryAmounts)
                    {
                        var nodes = ontologyData.GetNodesByUniqueValue(amountType.Id, militaryAmount.Name, "name");
                        var amountNode = nodes.FirstOrDefault();
                        if (amountNode == null) continue;

                        var codeNode = amountNode.GetSingleDirectProperty("code");
                        if (codeNode?.Value == militaryAmount.Code) continue;

                        if (codeNode != null) ontologyData.RemoveNode(codeNode.Id);
                        ontologyData.CreateRelationWithAttribute(amountNode.Id, codeRelationType.Id, militaryAmount.Code);

                        for (int i = 1; i < nodes.Count; i++)
                        {
                            ontologyData.RemoveNode(nodes[i].Id);
                        }
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
    }
}
