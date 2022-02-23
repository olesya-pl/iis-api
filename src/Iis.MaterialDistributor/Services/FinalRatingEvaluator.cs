using Iis.Interfaces.SecurityLevels;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.Services
{
    public class FinalRatingEvaluator : IFinalRatingEvaluator
    {
        private const int DefaultCoefficient = 1;
        private readonly ISecurityLevelChecker _securityLevelChecker;
        private readonly IChannelCoefficientEvaluator _channelCoefficientEvaluator;

        public FinalRatingEvaluator(
            ISecurityLevelChecker securityLevelChecker,
            IChannelCoefficientEvaluator channelCoefficientEvaluator)
        {
            _securityLevelChecker = securityLevelChecker;
            _channelCoefficientEvaluator = channelCoefficientEvaluator;
        }

        public decimal GetFinalRating(MaterialDistributionInfo material, UserDistributionInfo user)
        {
            if (!_securityLevelChecker.AccessGranted(user.SecurityLevels, material.SecurityLevels)) return 0;

            var channelCoefficient = _channelCoefficientEvaluator.Evaluate(material, user);

            return material.VariableCoefficient * (material.PermanentCoefficient ?? DefaultCoefficient) * channelCoefficient.Coefficient;
        }
    }
}
