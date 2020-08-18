using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Services;
using Iis.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Iis.UnitTests.Roles
{
    public class RolesServiceTests : IDisposable
    {
        public readonly ServiceProvider _serviceProvider;

        public RolesServiceTests()
        {
            _serviceProvider = Utils.GetServiceProvider();
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

            var roleEntity = context.Roles.Include(x => x.RoleGroups).Single(x => x.Id == role.Id);
            Assert.Equal(roleEntity.RoleGroups.Count, role.ActiveDirectoryGroupIds.Count);
        }        

        [Theory, RecursiveAutoData]
        public async Task Update_UpdatedOnlyOneAccess(RoleEntity role,
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
                AccessGrantedItems = new AccessGrantedList().Merge(existingAccesses
                    .Select(p => new AccessGranted
                    {
                        Id = p.Id,
                        Kind = p.Kind,
                        Title = p.Title,
                        Category = p.Category,
                        CreateGranted = true,
                        DeleteGranted = true,
                        UpdateGranted = true
                    }))
            });

            //assert
            Assert.Equal(3, result.AccessGrantedItems.Count);
            Assert.False(result.AccessGrantedItems.First().ReadGranted);
            Assert.True(result.AccessGrantedItems.First().UpdateGranted);
            Assert.True(result.AccessGrantedItems.First().CreateGranted);
            Assert.True(result.AccessGrantedItems.First().DeleteGranted);

            Assert.Equal("updated", result.Description);
            Assert.Equal("updated_name", result.Name);
        }

        [Theory, RecursiveAutoData]
        public async Task Update_ActiveDirectoryGroups(RoleEntity role)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            role.RoleAccessEntities = new List<RoleAccessEntity>();
            role.RoleGroups = new List<RoleActiveDirectoryGroupEntity> 
            {
                new RoleActiveDirectoryGroupEntity
                {
                    Id = Guid.NewGuid(),
                    GroupId = Guid.Parse("60B3A3A7-12BC-41AF-9C25-502E594A775F")
                }
            };
            context.Roles.Add(role);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<RoleService>();

            var result = await sut.UpdateRoleAsync(new Role
            {
                Id = role.Id,
                Description = "updated",
                Name = "updated_name",
                IsAdmin = false,
                ActiveDirectoryGroupIds = new List<Guid>
                {
                    Guid.Parse("79F363CD-D2F5-4D19-B0EE-ACA8E6872746")
                }
            });

            //assert
            Assert.Single(result.ActiveDirectoryGroupIds);
            Assert.Contains(Guid.Parse("79F363CD-D2F5-4D19-B0EE-ACA8E6872746"), result.ActiveDirectoryGroupIds);
            Assert.DoesNotContain(Guid.Parse("60B3A3A7-12BC-41AF-9C25-502E594A775F"), result.ActiveDirectoryGroupIds);
        }
    }
}
