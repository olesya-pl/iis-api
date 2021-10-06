using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Services;
using Microsoft.EntityFrameworkCore;
using Iis.Domain.Users;

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
            var (obtainedRole, alreadyExists) = await sut.CreateRoleAsync(role);

            //assert
            Assert.Equal(existingAccesses.Count, context.AccessObjects.Count());
            var accessRoles = context.RoleAccess
                .Where(p => p.RoleId == role.Id && existingAccesses.Select(p => p.Id).Contains(p.AccessObjectId))
                .ToList();
            Assert.Equal(existingAccesses.Count, accessRoles.Count());

            var roleEntity = context.Roles.Include(x => x.RoleGroups).Single(x => x.Id == role.Id);
            Assert.Equal(roleEntity.RoleGroups.Count, role.ActiveDirectoryGroupIds.Count);
            Assert.Equal(role.Id, obtainedRole.Id);
            Assert.Equal(false, alreadyExists);
        }

        [Theory, RecursiveAutoData]
        public async Task Create_RoleExists_Should_DoNothing(RoleEntity role, RoleEntity existedRole, string roleName)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            InitRoleEntity(role);
            InitRoleEntity(existedRole);
            existedRole.Name = roleName;
            context.Roles.Add(existedRole);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<RoleService>();

            var (obtainedRole, alreadyExists) = await sut.CreateRoleAsync(new Role
            {
                Id = role.Id,
                Description = role.Description,
                Name = roleName,
                IsAdmin = false,
                ActiveDirectoryGroupIds = new List<Guid>
                {
                    Guid.Parse("79F363CD-D2F5-4D19-B0EE-ACA8E6872746")
                },
                AccessGrantedItems = new AccessGrantedList()
            });

            //assert
            Assert.Equal(null, obtainedRole);
            Assert.Equal(true, alreadyExists);
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

            var (result, alreadyExists) = await sut.UpdateRoleAsync(new Role
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
                        UpdateGranted = true
                    }))
            });

            //assert
            Assert.Equal(3, result.AccessGrantedItems.Count);
            Assert.False(result.AccessGrantedItems.First().ReadGranted);
            Assert.True(result.AccessGrantedItems.First().UpdateGranted);
            Assert.True(result.AccessGrantedItems.First().CreateGranted);

            Assert.Equal("updated", result.Description);
            Assert.Equal("updated_name", result.Name);
            Assert.Equal(false, alreadyExists);
        }

        [Theory, RecursiveAutoData]
        public async Task Update_ActiveDirectoryGroups(RoleEntity role)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            InitRoleEntity(role);
            context.Roles.Add(role);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<RoleService>();

            var (obtainedRole, alreadyExists) = await sut.UpdateRoleAsync(new Role
            {
                Id = role.Id,
                Description = "updated",
                Name = "updated_name",
                IsAdmin = false,
                ActiveDirectoryGroupIds = new List<Guid>
                {
                    Guid.Parse("79F363CD-D2F5-4D19-B0EE-ACA8E6872746")
                },
                AccessGrantedItems = new AccessGrantedList()
            });

            //assert
            Assert.Single(obtainedRole.ActiveDirectoryGroupIds);
            Assert.Contains(Guid.Parse("79F363CD-D2F5-4D19-B0EE-ACA8E6872746"), obtainedRole.ActiveDirectoryGroupIds);
            Assert.DoesNotContain(Guid.Parse("60B3A3A7-12BC-41AF-9C25-502E594A775F"), obtainedRole.ActiveDirectoryGroupIds);
        }

        [Theory, RecursiveAutoData]
        public async Task Update_RoleNameExisted_Should_DoNoChanges(RoleEntity role, RoleEntity existedRole, string roleName)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            InitRoleEntity(role);
            InitRoleEntity(existedRole);
            existedRole.Name = roleName;
            context.Roles.AddRange(role, existedRole);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<RoleService>();

            var (obtainedRole, alreadyExists) = await sut.UpdateRoleAsync(new Role
            {
                Id = role.Id,
                Description = "updated",
                Name = roleName,
                IsAdmin = false,
                ActiveDirectoryGroupIds = new List<Guid>
                {
                    Guid.Parse("79F363CD-D2F5-4D19-B0EE-ACA8E6872746")
                },
                AccessGrantedItems = new AccessGrantedList()
            });

            //assert
            Assert.Equal(role.Id, obtainedRole.Id);
            Assert.Equal(role.Name, obtainedRole.Name);
            Assert.Equal(role.Description, obtainedRole.Description);
            Assert.Equal(true, alreadyExists);
        }

        private void InitRoleEntity(RoleEntity role)
        {
            role.RoleAccessEntities = new List<RoleAccessEntity>();
            role.RoleGroups = new List<RoleActiveDirectoryGroupEntity>
            {
                new RoleActiveDirectoryGroupEntity
                {
                    Id = Guid.NewGuid(),
                    GroupId = Guid.Parse("60B3A3A7-12BC-41AF-9C25-502E594A775F")
                }
            };
        }
    }
}
