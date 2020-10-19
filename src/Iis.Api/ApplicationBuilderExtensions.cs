using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iis.DataModel;
using Iis.Domain;
using Iis.Interfaces.Elastic;
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
                    var ontologyModel = serviceProvider.GetRequiredService<IOntologyModel>();
                    var ontologyService = serviceProvider.GetRequiredService<IOntologyService>();
                    var context = serviceProvider.GetRequiredService<OntologyContext>();

                    var amountType = ontologyModel.EntityTypes.FirstOrDefault(p => p.Name == "MilitaryAmount");

                    if (amountType == null)
                    {
                        return;
                    }

                    if (!File.Exists("data/contour/entities/MilitaryAmount.json"))
                    {
                        return;
                    }

                    var text = File.ReadAllText("data/contour/entities/MilitaryAmount.json");
                    var militaryAmounts = JsonConvert.DeserializeObject<List<MilitaryAmountEntry>>(text);

                    foreach (var militaryAmount in militaryAmounts)
                    {
                        var nodes = ontologyService.GetEntitiesByUniqueValue(amountType.Id, militaryAmount.Name, "name")
                            .GetAwaiter().GetResult();
                        var amount = nodes.First();
                        amount.SetProperty("code", militaryAmount.Code);
                        amount.SetProperty("name", militaryAmount.Name);
                        ontologyService.SaveNodeAsync(amount).GetAwaiter().GetResult();

                        var redundantAmountIds = nodes.Skip(1).Select(p => p.Id).ToList();
                        if (redundantAmountIds.Any())
                        {
                            var relationsToUpdate = context.Relations.Where(p => redundantAmountIds.Contains(p.TargetNodeId));
                            foreach (var relation in relationsToUpdate)
                            {
                                relation.TargetNodeId = amount.Id;
                            }
                            context.Nodes.RemoveRange(context.Nodes.Where(p => redundantAmountIds.Contains(p.Id)));
                            context.SaveChanges();
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
