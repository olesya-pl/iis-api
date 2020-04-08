using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Roles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.Roles
{
    public class RolesSaverTests
    {
        public readonly ServiceProvider _serviceProvider;

        public RolesSaverTests()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
        }

        [Theory, RecursiveAutoData]
        public async Task CreateRole_RespectsExistingAccessObjects(Role role,
            List<AccessObjectEntity> existingAccesses)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.AccessObjects.AddRange(existingAccesses);
            context.SaveChanges();

            role.AccessGrantedItems = new AccessGrantedList().Merge(existingAccesses.Select(p => new AccessGranted
            {
               Category = p.Category,
               Id = p.Id,
               Kind = p.Kind,
               Title = p.Title
            }));
            //act
            var sut = _serviceProvider.GetRequiredService<RoleSaver>();
            var result = await sut.CreateRoleAsync(role);

            //assert
            Assert.Equal(existingAccesses.Count, context.AccessObjects.Count());
            var accessRoles = context.RoleAccess
                .Where(p => p.RoleId == role.Id && existingAccesses.Select(p => p.Id).Contains(p.AccessObjectId))
                .ToList();
            Assert.Equal(existingAccesses.Count, accessRoles.Count());
        }
    }
}
