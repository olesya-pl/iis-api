using System;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public class VariableCoefficientRule
    {
        private Func<int, bool> ruleFunction;

        public VariableCoefficientRule(VariableCoefficient fromCoefficient, VariableCoefficient toCoefficient)
        {
            Coefficient = fromCoefficient?.Coefficient ?? 0;

            ruleFunction = GetRuleFunction(fromCoefficient?.OffsetHours, toCoefficient?.OffsetHours);
        }

        public int Coefficient { get; }

        public bool IsSatisfied(int dateDifferenceHours)
        {
            if (ruleFunction is null) return false;

            return ruleFunction(dateDifferenceHours);
        }

        private static Func<int, bool> GetRuleFunction(int? fromOffset, int? toOffset) => (fromOffset, toOffset) switch
        {
            (null, null) => null,
            (_, null) => dateDiff => dateDiff == fromOffset,
            (0, _) => dateDiff => dateDiff < toOffset,
            (_, _) => dateDiff => dateDiff >= fromOffset && dateDiff < toOffset
        };
    }
}