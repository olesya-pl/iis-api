using System.Collections.Generic;
using Iis.MaterialDistributor.Contracts.Services.DataTypes;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IPermanentCoefficientEvaluator
    {
        IReadOnlyCollection<MaterialPermanentCoefficient> Evaluate(IReadOnlyDictionary<string, int> coefficients, IReadOnlyCollection<MaterialInfo> materialInfoCollection);
    }
}