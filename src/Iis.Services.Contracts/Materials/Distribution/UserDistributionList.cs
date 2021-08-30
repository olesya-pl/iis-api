using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class UserDistributionList
    {
        public List<UserDistributionDto> Items { get; set; } = new List<UserDistributionDto>();
        public UserDistributionList() { }
        public UserDistributionList(IEnumerable<UserDistributionDto> items)
        {
            Items = items.ToList();
        }

        public UserDistributionDto GetUser(string roleName)
        {
            var user = Items
                .Where(_ => _.FreeSlots > 0)
                .OrderByDescending(_ => _.GetPriority(roleName))
                .ThenBy(_ => _.FreeSlots)
                .FirstOrDefault();

            return user.GetPriority(roleName) == -1 ? null : user;
        }
    }
}
