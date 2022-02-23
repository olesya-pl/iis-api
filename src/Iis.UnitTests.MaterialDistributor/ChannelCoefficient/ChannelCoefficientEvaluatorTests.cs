using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;

namespace Iis.UnitTests.MaterialDistributor
{
    public class ChannelCoefficientEvaluatorTests
    {
        [Fact]
        public void EvaluateReturnsDefaultUserChannelInfoWhenMaterialIsNull()
        {
            new ChannelCoefficientEvaluator()
                .Evaluate(null, new UserDistributionInfo())
                .Should().BeEquivalentTo(UserChannelInfo.Default);
        }

        [Fact]
        public void EvaluateReturnsDefaultUserChannelInfoWhenMaterialChannelIsNullOrEmpty()
        {
            new ChannelCoefficientEvaluator()
                .Evaluate(new MaterialDistributionInfo(), new UserDistributionInfo())
                .Should().BeEquivalentTo(UserChannelInfo.Default);
        }

        [Fact]
        public void EvaluateReturnsDefaultUserChannelInfoWhenUserDistributionInfoIsNull()
        {
            new ChannelCoefficientEvaluator()
                .Evaluate(new MaterialDistributionInfo(), null)
                .Should().BeEquivalentTo(UserChannelInfo.Default);
        }

        [Theory, AutoMoqData]
        public void EvaluateReturnsDefaultUserChannelInfoWhenNoUserCoefficientData(MaterialDistributionInfo material, UserDistributionInfo user)
        {
            new ChannelCoefficientEvaluator()
                .Evaluate(material, user)
                .Should().BeEquivalentTo(UserChannelInfo.Default);
        }

        [Theory, AutoMoqData]
        public void EvaluateReturnsDefaultUserChannelInfoWhenDontFindByUserId(MaterialDistributionInfo material, UserDistributionInfo user, IReadOnlyCollection<UserDistributionInfo> userDistributionInfos)
        {
            user.Id = Guid.NewGuid();

            new ChannelCoefficientEvaluator(userDistributionInfos)
                .Evaluate(material, user)
                .Should().BeEquivalentTo(UserChannelInfo.Default);
        }

        [Theory, AutoMoqData]
        public void EvaluateReturnsDefaultUserChannelWhenUserMatchesButMaterialChannelDoesnt(MaterialDistributionInfo material, UserDistributionInfo user, IReadOnlyList<UserDistributionInfo> userDistributionInfos)
        {
            var index = new Random().Next(0, userDistributionInfos.Count);

            var matchedUser = userDistributionInfos[index];

            matchedUser.Id = user.Id;

            material.Channel = Guid.NewGuid().ToString();

            new ChannelCoefficientEvaluator(userDistributionInfos)
                .Evaluate(material, user)
                .Should().BeEquivalentTo(UserChannelInfo.Default);
        }

        [Theory, AutoMoqData]
        public void EvaluateReturnsExpectedUserChannelWhenUserAndMateriaChannelMatches(MaterialDistributionInfo material, UserDistributionInfo user, IReadOnlyList<UserDistributionInfo> userDistributionInfos)
        {
            var index = new Random().Next(0, user.Channels.Count);

            var expectedChannel = user.Channels[index];

            material.Channel = expectedChannel.Channel;

            var newUserDistributionInfos = new List<UserDistributionInfo>(userDistributionInfos)
            {
                user
            };

            var evaluator = new ChannelCoefficientEvaluator(userDistributionInfos);

            evaluator.ReloadUserDistributionInfos(newUserDistributionInfos);

            evaluator.Evaluate(material, user)
                .Should().BeEquivalentTo(expectedChannel);
        }
    }
}