using System;
using System.Collections.Generic;
using System.Linq;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Contracts.Services.DataTypes;
using Iis.MaterialDistributor.DataModel.Entities;

namespace Iis.MaterialDistributor.PermanentCoefficients
{
    public class PermanentCoefficientEvaluator : IPermanentCoefficientEvaluator
    {
        private const int DefaultCoefficientValue = 0;

        private readonly IReadOnlyDictionary<string, PermanentCoefficientRule> _rules = new Dictionary<string, PermanentCoefficientRule>
        {
            { PermanentCoefficientEntity.RelatedToObjectOfStudy, new RelatedToObjectOfStudyRule() },
            { PermanentCoefficientEntity.HasPhoneNumber, new HasPhoneNumberRule() },
            { PermanentCoefficientEntity.HasIridiumOptions, new HasIridiumOptionsRule() },
            { PermanentCoefficientEntity.HasTMSI, new HasTMSIRule() },
            { PermanentCoefficientEntity.RelatedWithHighPriority, new RelatedWithHighPriority() },
            { PermanentCoefficientEntity.RelatedAndIgnoredPriority, new RelatedAndIgnoredRule() },
        };

        public IReadOnlyCollection<MaterialPermanentCoefficient> Evaluate(IReadOnlyDictionary<string, int> coefficients, IReadOnlyCollection<MaterialInfo> materialInfoCollection)
        {
            if (materialInfoCollection.Count == 0) return Array.Empty<MaterialPermanentCoefficient>();

            return materialInfoCollection
                .Select(_ => Evaluate(coefficients, _))
                .ToArray();
        }

        private MaterialPermanentCoefficient Evaluate(IReadOnlyDictionary<string, int> coefficients, MaterialInfo materialInfo)
        {
            var materialCoefficient = _rules.Sum(_ => _.Value.IsSatisfied(materialInfo) ? coefficients.GetValueOrDefault(_.Key, DefaultCoefficientValue) : 0);

            return new MaterialPermanentCoefficient
            {
                Id = materialInfo.Material.Id,
                Value = materialCoefficient
            };
        }
    }
}