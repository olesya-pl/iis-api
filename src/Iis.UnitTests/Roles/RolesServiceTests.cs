using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Iis.Roles;
using Iis.DataModel;
using Iis.DataModel.Roles;

namespace Iis.UnitTests.Roles
{
    public class RolesServiceTests : IDisposable
    {
        public readonly ServiceProvider _serviceProvider;

        public RolesServiceTests()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
        }

        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.AccessObjects.RemoveRange(context.AccessObjects);
            context.SaveChanges();

            _serviceProvider.Dispose();
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
            var sut = _serviceProvider.GetRequiredService<RoleService>();
            var result = await sut.CreateRoleAsync(role);

            //assert
            Assert.Equal(existingAccesses.Count, context.AccessObjects.Count());
            var accessRoles = context.RoleAccess
                .Where(p => p.RoleId == role.Id && existingAccesses.Select(p => p.Id).Contains(p.AccessObjectId))
                .ToList();
            Assert.Equal(existingAccesses.Count, accessRoles.Count());
        }        

        [Theory, RecursiveAutoData]
        public async Task Update_SavesOnlyProvidedAccesses(RoleEntity role,
            List<AccessObjectEntity> existingAccesses)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            role.RoleAccessEntities = new List<RoleAccessEntity>();
            context.Roles.Add(role);
            context.AccessObjects.AddRange(existingAccesses);
            context.SaveChanges();
            
            foreach (var access in existingAccesses)
            {
                context.RoleAccess.Add(new RoleAccessEntity
                {
                    AccessObjectId = access.Id,
                    RoleId = role.Id
                });
            }
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<RoleService>();
            var result = await sut.UpdateRoleAsync(new Role
            {
                Id = role.Id,
                Description = "updated",
                Name = "updated_name",
                IsAdmin = false,
                AccessGrantedItems = new AccessGrantedList()
                .Merge(new List<AccessGranted> { existingAccesses
                    .Select(p => new AccessGranted {
                        Id = p.Id,
                        Kind = p.Kind,
                        Title = p.Title,
                        Category = p.Category,
                        CreateGranted = true,
                        DeleteGranted = true,
                        UpdateGranted = true
                    })
                    .First() })
            });

            //assert
            var accessRoles = context.RoleAccess
                .Where(p => p.RoleId == role.Id && existingAccesses.Select(p => p.Id).Contains(p.AccessObjectId))
                .ToList();
            Assert.Single(accessRoles);
            Assert.False(accessRoles.First().ReadGranted);
            Assert.True(accessRoles.First().UpdateGranted);
            Assert.True(accessRoles.First().CreateGranted);
            Assert.True(accessRoles.First().DeleteGranted);

            Assert.Equal("updated", result.Description);
            Assert.Equal("updated_name", result.Name);
        }
    }
}
