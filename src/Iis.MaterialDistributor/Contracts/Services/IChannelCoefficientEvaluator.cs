using System.Collections.Generic;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IChannelCoefficientEvaluator
    {
        void ReloadUserDistributionInfos(IReadOnlyCollection<UserDistributionInfo> source);

        UserChannelInfo Evaluate(MaterialDistributionInfo materialInfo, UserDistributionInfo userInfo);
    }
}