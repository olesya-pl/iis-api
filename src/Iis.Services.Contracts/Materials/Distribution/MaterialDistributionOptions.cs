using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public enum DistributionStrategy
    {
        InSuccession,
        Evenly
    }
    public class MaterialDistributionOptions
    {
        public DistributionStrategy Strategy { get; set; }
    }
}
