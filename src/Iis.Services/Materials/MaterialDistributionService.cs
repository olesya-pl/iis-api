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
            MaterialDistributionItem material,
            UserDistributionList users)
        {
            var user = GetUser(users, material.RoleName);
            if (user == null) return null;
            user.FreeSlots--;
            return new DistributionResultItem(material.Id, user.Id);
        }

        private List<MaterialDistributionItem> ShuffleEvenly(List<MaterialDistributionItem> baseItems)
        {
            if (baseItems.Count <= 1) return baseItems;
            var result = new List<MaterialDistributionItem>();
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

        private UserDistributionItem GetUser(UserDistributionList users, string roleName)
        {
            var user = users.Items
                .Where(_ => _.FreeSlots > 0)
                .OrderByDescending(_ => GetPriority(_, roleName))
                .ThenBy(_ => _.FreeSlots)
                .FirstOrDefault();

            return user == null || GetPriority(user, roleName) == -1 ? null : user;
        }

        private int GetPriority(UserDistributionItem user, string roleName)
        {
            if (user.RoleNames.Count == 0) return 0;

            if (string.IsNullOrEmpty(roleName))
                return user.RoleNames.Count == 0 ? 0 : 1;

            if (!user.RoleNames.Contains(roleName)) return -1;
            if (user.RoleNames.Count == 1) return 2;
            return 1;
        }
    }
}
