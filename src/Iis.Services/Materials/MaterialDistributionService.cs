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
            foreach (var material in materials.Items)
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
    }
}
