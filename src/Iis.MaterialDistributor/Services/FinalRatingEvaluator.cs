using Iis.Interfaces.SecurityLevels;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.DataStorage;

namespace Iis.MaterialDistributor.Services
{
    public class FinalRatingEvaluator : IFinalRatingEvaluator
    {
        private readonly ISecurityLevelChecker _securityLevelChecker;

        public FinalRatingEvaluator(ISecurityLevelChecker securityLevelChecker)
        {
            _securityLevelChecker = securityLevelChecker;
        }

        public decimal GetFinalRating(MaterialDistributionInfo material, UserDistributionInfo user)
        {
            if (!_securityLevelChecker.AccessGranted(user.SecurityLevels, material.SecurityLevels)) return 0;

            return material.VariableCoefficient * 1;
        }
    }
}
