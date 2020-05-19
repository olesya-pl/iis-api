using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Iis.DataModel;
using Iis.Roles;
using Iis.UnitTests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.Users
{
    public class OperatorsTests : IDisposable
    {
        public readonly ServiceProvider _serviceProvider;

        public OperatorsTests()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
        }

        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            _serviceProvider.Dispose();
        }

        [Theory, RecursiveAutoData]
        public async Task GetOperators_ReturnsAllUsers(List<UserEntity> data)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.AddRange(data);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<UserService>();
            var result = await sut.GetOperatorsAsync();

            //assert
            Assert.Equal(data.Count, result.Count);
            foreach (var assignee in result)
            {
                var dataItem = data.First(p => p.Id == assignee.Id);
                UserTestHelper.AssertUserEntityMappedToUserCorrectly(dataItem, assignee);
            }
        }
    }
}
