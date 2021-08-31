using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Materials.Distribution;
using Iis.Services.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var materials = new MaterialDistributionList();
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, null)
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Empty(actual.Items);
        }
        [Fact]
        public void Distribute_NoUsers()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, null, null)
            });
            var users = new UserDistributionList();
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Empty(actual.Items);
        }
        [Fact]
        public void Distribute_OneWithoutRole_OneUser_1()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, null, null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, null)
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Single(actual.Items);
            Assert.Equal(materials.Items[0].Id, actual.Items[0].MaterialId);
            Assert.Equal(users.Items[0].Id, actual.Items[0].UserId);
        }
        [Fact]
        public void Distribute_OneWithoutRole_OneUser_2()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, null, null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string> { "Role1", "Role2" })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Single(actual.Items);
            Assert.Equal(materials.Items[0].Id, actual.Items[0].MaterialId);
            Assert.Equal(users.Items[0].Id, actual.Items[0].UserId);
        }
        [Fact]
        public void Distribute_OneWithRole_OneUser_1()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, null)
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Single(actual.Items);
            Assert.Equal(materials.Items[0].Id, actual.Items[0].MaterialId);
            Assert.Equal(users.Items[0].Id, actual.Items[0].UserId);
        }
        [Fact]
        public void Distribute_OneWithRole_OneUser_2()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role1" })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Single(actual.Items);
            Assert.Equal(materials.Items[0].Id, actual.Items[0].MaterialId);
            Assert.Equal(users.Items[0].Id, actual.Items[0].UserId);
        }
        [Fact]
        public void Distribute_OneWithRole_OneUser_3()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role2" })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Empty(actual.Items);
        }
        [Fact]
        public void Distribute_OneWithRole_OneUser_4()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role2", "Role3", "Role1" })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Single(actual.Items);
            Assert.Equal(materials.Items[0].Id, actual.Items[0].MaterialId);
            Assert.Equal(users.Items[0].Id, actual.Items[0].UserId);
        }
        [Fact]
        public void Distribute_OneWithRole_ManyUsers_1()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, null),
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role2", "Role3" }),
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role2", "Role3", "Role1" })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Single(actual.Items);
            Assert.Equal(materials.Items[0].Id, actual.Items[0].MaterialId);
            Assert.Equal(users.Items[2].Id, actual.Items[0].UserId);
        }
        [Fact]
        public void Distribute_OneWithRole_ManyUsers_2()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, null),
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role2", "Role3" }),
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role2", "Role3", "Role4" })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            Assert.Single(actual.Items);
            Assert.Equal(materials.Items[0].Id, actual.Items[0].MaterialId);
            Assert.Equal(users.Items[0].Id, actual.Items[0].UserId);
        }
        [Fact]
        public void Distribute_Many_FreeSlots_1()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 1, new List<string> { "Role1" }),
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role1" }),
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ "Role2" })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);

            var expected = new DistributionResult(new List<DistributionResultItem>
            {
                new DistributionResultItem(materials[0].Id, users[0].Id),
                new DistributionResultItem(materials[1].Id, users[1].Id),
                new DistributionResultItem(materials[2].Id, users[1].Id),
                new DistributionResultItem(materials[3].Id, users[1].Id),
            });

            AssertDistributionResult(expected, actual);
        }
        [Fact]
        public void Distribute_Many_FreeSlots_2()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role3", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role2", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role2", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ }),
                new UserDistributionItem(Guid.NewGuid(), 1, new List<string> { "Role1" }),
                new UserDistributionItem(Guid.NewGuid(), 1, new List<string>{ "Role2" }),
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            var expected = new DistributionResult(new List<DistributionResultItem>
            {
                new DistributionResultItem(materials[0].Id, users[1].Id),
                new DistributionResultItem(materials[1].Id, users[0].Id),
                new DistributionResultItem(materials[2].Id, users[0].Id),
                new DistributionResultItem(materials[3].Id, users[2].Id),
                new DistributionResultItem(materials[4].Id, users[0].Id),
            });
            AssertDistributionResult(expected, actual);
        }

        [Fact]
        public void Distribute_Many_Priority_1()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 2, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 3, "Role1", null),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", null)
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 10, new List<string>{ }),
                new UserDistributionItem(Guid.NewGuid(), 2, new List<string> { "Role1" }),
                new UserDistributionItem(Guid.NewGuid(), 1, new List<string>{ "Role2" }),
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            var expected = new DistributionResult(new List<DistributionResultItem>
            {
                new DistributionResultItem(materials[0].Id, users[0].Id),
                new DistributionResultItem(materials[1].Id, users[0].Id),
                new DistributionResultItem(materials[2].Id, users[1].Id),
                new DistributionResultItem(materials[3].Id, users[1].Id),
                new DistributionResultItem(materials[4].Id, users[0].Id),
            });
            AssertDistributionResult(expected, actual);
        }

        [Fact]
        public void Distribute_Many_Channels_1()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", "Channel1"),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", "Channel1"),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", "Channel1"),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", "Channel1"),
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role2", "Channel2")
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 1, new List<string>{ }),
                new UserDistributionItem(Guid.NewGuid(), 1, new List<string> { "Role1" })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            var expected = new DistributionResult(new List<DistributionResultItem>
            {
                new DistributionResultItem(materials[0].Id, users[1].Id),
                new DistributionResultItem(materials[4].Id, users[0].Id),
            });
            AssertDistributionResult(expected, actual);
        }

        [Fact]
        public void Distribute_Many_Channels_2()
        {
            var materials = new MaterialDistributionList(new List<MaterialDistributionItem>
            {
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", "Channel1"), // 0
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", "Channel1"), // 1
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role1", "Channel1"), // 2
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role2", "Channel2"), // 3
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role2", "Channel2"), // 4
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role2", "Channel2"), // 5
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role3", "Channel3"), // 6
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role3", "Channel3"), // 7
                new MaterialDistributionItem(Guid.NewGuid(), 1, "Role3", "Channel3"), // 8
            });
            var users = new UserDistributionList(new List<UserDistributionItem>
            {
                new UserDistributionItem(Guid.NewGuid(), 2, new List<string> { "Role1" }),
                new UserDistributionItem(Guid.NewGuid(), 2, new List<string> { "Role2" }),
                new UserDistributionItem(Guid.NewGuid(), 3, new List<string> {  })
            });
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var actual = GetService().Distribute(materials, users, options);
            var expected = new DistributionResult(new List<DistributionResultItem>
            {
                new DistributionResultItem(materials[0].Id, users[0].Id),
                new DistributionResultItem(materials[1].Id, users[0].Id),
                new DistributionResultItem(materials[2].Id, users[2].Id),
                new DistributionResultItem(materials[3].Id, users[1].Id),
                new DistributionResultItem(materials[4].Id, users[1].Id),
                new DistributionResultItem(materials[6].Id, users[2].Id),
                new DistributionResultItem(materials[7].Id, users[2].Id),
            });
            AssertDistributionResult(expected, actual);
        }

        private void AssertDistributionResult(DistributionResult expected, DistributionResult actual)
        {
            Assert.Equal(expected.Items.Count, actual.Items.Count);
            var expectedSorted = expected.Items.OrderBy(_ => _.MaterialId).ToList();
            var actualSorted = actual.Items.OrderBy(_ => _.MaterialId).ToList();
            for (int i = 0; i < expectedSorted.Count; i++)
            {
                Assert.True(actualSorted[i].IsEqual(expectedSorted[i]));
            }
        }
    }
}
