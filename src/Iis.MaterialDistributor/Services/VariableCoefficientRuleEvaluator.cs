using System;
using System.Linq;
using System.Collections.Generic;

namespace Iis.MaterialDistributor.Contracts.Services
{
    internal class VariableCoefficientRuleEvaluator : IVariableCoefficientRuleEvaluator
    {
        private const int DefaultVariableCoefficientValue = 0;

        public int GetVariableCoefficientValue(IReadOnlyCollection<VariableCoefficientRule> ruleCollection, DateTime comparisonTimeStamp, MaterialDistributionInfo document)
        {
            if (ruleCollection is null || ruleCollection.Count == 0 || document is null) return DefaultVariableCoefficientValue;

            var documentTimeStamp = document.RegistrationDate ?? document.CreatedDate;

            var dateDiff = (int)Math.Truncate((comparisonTimeStamp - documentTimeStamp).TotalHours);

            var rule = ruleCollection
                         .Where(_ => _.IsSatisfied(dateDiff))
                         .OrderByDescending(_ => _.Coefficient)
                         .FirstOrDefault();

            return rule is null ? DefaultVariableCoefficientValue : rule.Coefficient;
        }
    }
}