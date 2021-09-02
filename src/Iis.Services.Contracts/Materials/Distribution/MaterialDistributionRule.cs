using Iis.DataModel.Materials;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class MaterialDistributionRule
    {
        public int Priority { get; set; }
        public Expression<Func<MaterialEntity, bool>> Filter { get; set; }
        public Func<MaterialEntity, string> GetChannel { get; set; }
    }
}
