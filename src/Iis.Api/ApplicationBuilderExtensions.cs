using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iis.Domain;
using Microsoft.AspNetCore.Builder;
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
                        nodes.Select(amount => {
                                amount.SetProperty("code", militaryAmount.Code);
                                amount.SetProperty("name", militaryAmount.Name);
                                ontologyService.SaveNodeAsync(amount).GetAwaiter().GetResult();
                                return amount;
                            });

                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
