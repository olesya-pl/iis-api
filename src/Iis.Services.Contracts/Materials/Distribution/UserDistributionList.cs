using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class UserDistributionList
    {
        public List<UserDistributionItem> Items { get; set; } = new List<UserDistributionItem>();
        public UserDistributionItem this[int index] => Items[index];
        public UserDistributionList() { }
        public UserDistributionList(IEnumerable<UserDistributionItem> items)
        {
            Items = items.ToList();
        }

        public int TotalFreeSlots() => Items.Sum(_ => _.FreeSlots);

        public override string ToString() =>
            $"Count: {Items.Count}";
    }
}
