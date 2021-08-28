using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Materials.Distribution;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Materials
{
    public class MaterialDistributionService: IMaterialDistributionService
    {
        public DistributionResult Distribute(
            MaterialDistributionList materials, 
            UserDistributionList users,
            MaterialDistributionOptions options)
        {
            return new DistributionResult();
        }
    }
}
