using Iis.Services.Contracts.Materials.Distribution;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IMaterialDistributionService
    {
        DistributionResult Distribute(
            MaterialDistributionList materials,
            UserDistributionList users,
            MaterialDistributionOptions options);
    }
}
