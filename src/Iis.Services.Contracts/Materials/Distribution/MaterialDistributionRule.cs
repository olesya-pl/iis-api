using Iis.DataModel.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class MaterialDistributionRule
    {
        public int Priority { get; set; }
        //public int GroupId { get; set; }
        public Func<MaterialEntity, MaterialDistributionDto> Filter { get; set; }
    }
}
