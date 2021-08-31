using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class MaterialDistributionList
    {
        public List<MaterialDistributionItem> Items { get; set; } = new List<MaterialDistributionItem>();
        public MaterialDistributionItem this[int index] => Items[index];
        public MaterialDistributionList() { }
        public MaterialDistributionList(IEnumerable<MaterialDistributionItem> items)
        {
            Items = items.ToList();
        }

        public override string ToString() =>
            $"Count: {Items.Count}";
    }
}
