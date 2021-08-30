using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class MaterialDistributionList
    {
        public List<MaterialDistributionDto> Items { get; set; } = new List<MaterialDistributionDto>();
        public MaterialDistributionList() { }
        public MaterialDistributionList(IEnumerable<MaterialDistributionDto> items)
        {
            Items = items.ToList();
        }

        public bool TotallyDistributed(DistributionResult distributionResult) =>
            Items.All(_ => distributionResult.Contains(_.Id));
        
    }
}
