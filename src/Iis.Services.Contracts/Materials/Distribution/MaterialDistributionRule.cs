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
        public Func<int, Task<IEnumerable<MaterialEntity>>> GetMaterials { get; set; }
        public Func<MaterialEntity, string> GetRole { get; set; }
        public Func<Task<IEnumerable<string>>> GetChannels { get; set; }
        public Func<int, string, Task<IEnumerable<MaterialEntity>>> GetMaterialsByChannel { get; set; }
    }
}
