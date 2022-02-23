using System;
using System.Linq;
using System.Collections.Generic;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.Services
{
    public class ChannelCoefficientEvaluator : IChannelCoefficientEvaluator
    {
        private IReadOnlyDictionary<Guid, UserChannelCoefficientData> _userCoefficientDatas;

        public ChannelCoefficientEvaluator()
        : this(Array.Empty<UserDistributionInfo>())
        {
        }

        public ChannelCoefficientEvaluator(IReadOnlyCollection<UserDistributionInfo> source)
        {
            ReloadUserDistributionInfos(source);
        }

        public void ReloadUserDistributionInfos(IReadOnlyCollection<UserDistributionInfo> source)
        {
            _userCoefficientDatas = source
                                        .Select(_ => new UserChannelCoefficientData(_))
                                        .ToDictionary(_ => _.UserId);
        }

        public UserChannelInfo Evaluate(MaterialDistributionInfo materialInfo, UserDistributionInfo userInfo)
        {
            if (CouldNotBeUsedForEvaluation(materialInfo) || CouldNotBeUsedForEvaluation(userInfo)) return UserChannelInfo.Default;

            return _userCoefficientDatas.TryGetValue(userInfo.Id, out UserChannelCoefficientData userData)
                ? userData.Get(materialInfo.Channel)
                : UserChannelInfo.Default;
        }

        private static bool CouldNotBeUsedForEvaluation(MaterialDistributionInfo materialInfo)
        {
            return materialInfo is null || string.IsNullOrWhiteSpace(materialInfo.Channel);
        }

        private static bool CouldNotBeUsedForEvaluation(UserDistributionInfo userInfo)
        {
            return userInfo is null;
        }
    }
}