using Iis.DataModel.Materials;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class MaterialDistributionRule
    {
        public int Priority { get; set; }
        public Func<int, Task<IEnumerable<MaterialEntity>>> Getter { get; set; }
    }
}
