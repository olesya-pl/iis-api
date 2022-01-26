using System;
using System.Collections.Generic;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IVariableCoefficientRuleEvaluator
    {
        int GetVariableCoefficientValue(IReadOnlyCollection<VariableCoefficientRule> ruleCollection, DateTime comparisonTimeStamp, MaterialDocument document);
    }
}