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
            IEnumerable<MaterialDistributionDto> materials, 
            IEnumerable<UserDistributionDto> users,
            MaterialDistributionOptions options)
        {
            return new DistributionResult();
        }
    }
}
