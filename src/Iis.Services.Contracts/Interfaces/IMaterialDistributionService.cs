using Iis.Services.Contracts.Materials.Distribution;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IMaterialDistributionService
    {
        DistributionResult Distribute(
            IEnumerable<MaterialDistributionDto> materials,
            IEnumerable<UserDistributionDto> users,
            MaterialDistributionOptions options);
    }
}
