using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IVariableCoefficientService
    {
        Task<VariableCoefficient> GetWithMaxOffsetHoursAsync(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<MaterialDocument>> SetForMaterialsAsync(
            DateTime comparisonTimeStamp,
            IReadOnlyCollection<MaterialDocument> materialCollection,
            CancellationToken cancellationToken);
    }
}