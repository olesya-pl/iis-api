using System.Collections.Generic;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IPermanentCoefficientEvaluator
    {
        IReadOnlyCollection<MaterialPermanentCoefficient> Evaluate(IReadOnlyDictionary<string, int> coefficients, IReadOnlyCollection<MaterialInfo> materialInfoCollection);
    }
}