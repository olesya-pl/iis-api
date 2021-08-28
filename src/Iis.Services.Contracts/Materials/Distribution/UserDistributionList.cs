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
    }
}
