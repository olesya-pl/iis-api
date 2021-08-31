using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Materials.Distribution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Materials
{
    public class MaterialDistributionService: IMaterialDistributionService
    {
        public DistributionResult Distribute(
            MaterialDistributionList materials,
            UserDistributionList users,
            MaterialDistributionOptions options)
        {
            var result = new DistributionResult();
            
            var items = options.Strategy == DistributionStrategy.Evenly ?
                ShuffleEvenly(materials.Items) :
                materials.Items;

            foreach (var material in items)
            {
                var item = DistributeOne(material, users);
                if (item != null)
                    result.Items.Add(item);
            }
            return result;
        }

        private DistributionResultItem DistributeOne(
            MaterialDistributionDto material,
            UserDistributionList users)
        {
            var user = users.GetUser(material.RoleName);
            if (user == null) return null;
            user.FreeSlots--;
            return new DistributionResultItem(material.Id, user.Id);
        }

        private List<MaterialDistributionDto> ShuffleEvenly(List<MaterialDistributionDto> baseItems)
        {
            if (baseItems.Count <= 1) return baseItems;
            var result = new List<MaterialDistributionDto>();
            var priorityGroups = baseItems.GroupBy(_ => _.Priority).OrderByDescending(g => g.Key);

            var maxCount = priorityGroups.Select(_ => _.Count()).Max();
            foreach (var priorityGroup in priorityGroups)
            {
                var channelGroups = priorityGroup.GroupBy(_ => _.Channel).Select(_ => _.ToList()).ToList();
                for (int i = 0; i < maxCount; i++)
                {
                    foreach (var channelGroup in channelGroups)
                    {
                        if (i < channelGroup.Count())
                            result.Add(channelGroup[i]);
                    }
                }
            }

            return result;
        }
    }
}
