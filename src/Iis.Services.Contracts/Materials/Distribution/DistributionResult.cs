using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class DistributionResult
    {
        public IReadOnlyList<DistributionResultItem> Items { get; }
        public DistributionResult()
        {
            Items = new List<DistributionResultItem>();
        }
        public DistributionResult(IEnumerable<DistributionResultItem> items)
        {
            Items = items.ToList();
        }
    }
}
