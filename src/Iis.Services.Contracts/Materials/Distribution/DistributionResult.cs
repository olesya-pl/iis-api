using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class DistributionResult
    {
        public List<DistributionResultItem> Items { get; }
        public DistributionResultItem this[int index] => Items[index];
        public DistributionResult()
        {
            Items = new List<DistributionResultItem>();
        }
        public DistributionResult(IEnumerable<DistributionResultItem> items)
        {
            Items = items.ToList();
        }

        public bool Contains(Guid materialId) =>
            Items.Any(_ => _.MaterialId == materialId);

        public Guid? GetUserId(Guid materialId) =>
            Items.SingleOrDefault(_ => _.MaterialId == materialId)?.UserId;

        public override string ToString() =>
            $"Count: {Items.Count}";
    }
}
