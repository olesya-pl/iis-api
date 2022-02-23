using System;
using Xunit;
using FluentAssertions;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;

namespace Iis.UnitTests.MaterialDistributor
{
    public class UserChannelCoefficientDataTests
    {
        [Theory, AutoMoqData]
        public void GetUserIdAndNameFromUserDistributionInfo(UserDistributionInfo userInfo)
        {
            var data = new UserChannelCoefficientData(userInfo);

            data.UserId.Should().Be(userInfo.Id);
            data.UserName.Should().Be(userInfo.UserName);
        }

        [Theory, AutoMoqData]
        public void GetDefaultUserChannelInfoWhenEmptyChannels(UserDistributionInfo userInfo)
        {
            userInfo.Channels = Array.Empty<UserChannelInfo>();

            var data = new UserChannelCoefficientData(userInfo);

            var result = data.Get(Guid.NewGuid().ToString());

            result.Should().BeEquivalentTo(UserChannelInfo.Default);
        }

        [Theory, AutoMoqData]
        public void GetDefaultUserChannelInfoWhenNullChannels(UserDistributionInfo userInfo)
        {
            userInfo.Channels = null;

            var data = new UserChannelCoefficientData(userInfo);

            var result = data.Get(Guid.NewGuid().ToString());

            result.Should().BeEquivalentTo(UserChannelInfo.Default);
        }

        [Theory, AutoMoqData]
        public void GetDefaultUserChannelInfoWhenNonEmptyChannels(UserDistributionInfo userInfo)
        {
            var index = new Random().Next(0, userInfo.Channels.Count);

            var expected = userInfo.Channels[index];

            var data = new UserChannelCoefficientData(userInfo);

            var result = data.Get(expected.Channel);

            result.Should().BeEquivalentTo(expected);
        }
    }
}