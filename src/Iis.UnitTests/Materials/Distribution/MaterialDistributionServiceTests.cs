using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Materials.Distribution;
using Iis.Services.Materials;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Iis.UnitTests.Materials.Distribution
{
    public class MaterialDistributionServiceTests
    {
        private IMaterialDistributionService GetService() => new MaterialDistributionService();
        [Fact]
        public void Distribute_NoMaterials()
        {
            var materials = new List<MaterialDistributionDto>();
            var users = new List<UserDistributionDto>
            {
                new UserDistributionDto(Guid.NewGuid(), 10, null)
            };
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.InSuccession };
            var result = GetService().Distribute(materials, users, options);
            Assert.Empty(result.Items);
        }
        public void Distribute_NoUsers()
        {
            var materials = new List<MaterialDistributionDto>
            {
                new MaterialDistributionDto(Guid.NewGuid(), null, null)
            };
            var users = new List<UserDistributionDto>
            {
                new UserDistributionDto(Guid.NewGuid(), 10, null)
            };
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.InSuccession };
            var result = GetService().Distribute(materials, users, options);
            Assert.Empty(result.Items);
        }
    }
}
