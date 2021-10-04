using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Services.Contracts.Interfaces;
using Iis.Domain.Users;

namespace Iis.UnitTests.UserManagement
{
    public class UserServiceCreateUpdate : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public UserServiceCreateUpdate()
        {
            _serviceProvider = Utils.GetServiceProvider();
        }
        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.AccessObjects.RemoveRange(context.AccessObjects);
            context.RoleAccess.RemoveRange(context.RoleAccess);
            context.UserRoles.RemoveRange(context.UserRoles);
            context.Roles.RemoveRange(context.Roles);
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            _serviceProvider.Dispose();
        }
        [Theory(DisplayName = "Create user"), RecursiveAutoData]
        public async Task CreateUser(User user,
            List<AccessObjectEntity> existingAccesses,
            RoleEntity roleEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();
            roleEntity.UserRoles = null;

            existingAccesses.ForEach(e => e.RoleAccessEntities = null);

            context.Roles.Add(roleEntity);
            context.AccessObjects.AddRange(existingAccesses);
            context.SaveChanges();

            foreach (var access in existingAccesses)
            {
                context.RoleAccess.Add(new RoleAccessEntity
                {
                    AccessObjectId = access.Id,
                    RoleId = roleEntity.Id
                });
            }
            context.SaveChanges();
            // arrange: end

            //act
            user.Roles = new List<Role> { new Role {Id = roleEntity.Id}};

            var newUserId = await service.CreateUserAsync(user);
            
            //assert
            Assert.Equal(user.Id, newUserId);
            Assert.Single(user.Roles);
            Assert.Equal(roleEntity.Id, user.Roles.FirstOrDefault().Id);
        }
        [Theory(DisplayName = "Update user with exception Cannot find user with Id"), RecursiveAutoData]
        public async Task UpdateUser_Exception_CannotFindUser(
            List<AccessObjectEntity> existingAccesses,
            RoleEntity roleEntity,
            UserEntity userEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            userEntity.UserRoles = null;
            roleEntity.UserRoles = null;
            existingAccesses.ForEach(e => e.RoleAccessEntities = null);

            context.Users.Add(userEntity);
            context.Roles.Add(roleEntity);
            context.UserRoles.Add(new UserRoleEntity { Id = Guid.NewGuid(), UserId = userEntity.Id, RoleId = roleEntity.Id });
            context.AccessObjects.AddRange(existingAccesses);
            context.SaveChanges();
            foreach (var access in existingAccesses)
            {
                context.RoleAccess.Add(new RoleAccessEntity
                {
                    AccessObjectId = access.Id,
                    RoleId = roleEntity.Id
                });
            }
            context.SaveChanges();
            // arrange: end

            var updatedUser = await service.GetUserAsync(userEntity.Id);
            
            updatedUser.Id = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateUserAsync(updatedUser));

            Assert.Equal($"Cannot find User with id:'{updatedUser.Id}'.", exception.Message);
        }
        [Theory(DisplayName = "Update user with exception new password must not match old"), RecursiveAutoData]
        public async Task UpdateUser_Exception_NewPasswordIsMatchedOld(
            List<AccessObjectEntity> existingAccesses,
            RoleEntity roleEntity,
            UserEntity userEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            userEntity.UserRoles = null;
            roleEntity.UserRoles = null;
            existingAccesses.ForEach(e => e.RoleAccessEntities = null);

            context.Users.Add(userEntity);
            context.Roles.Add(roleEntity);
            context.UserRoles.Add(new UserRoleEntity { Id = Guid.NewGuid(), UserId = userEntity.Id, RoleId = roleEntity.Id });
            context.AccessObjects.AddRange(existingAccesses);
            context.SaveChanges();
            foreach (var access in existingAccesses)
            {
                context.RoleAccess.Add(new RoleAccessEntity
                {
                    AccessObjectId = access.Id,
                    RoleId = roleEntity.Id
                });
            }
            context.SaveChanges();
            // arrange: end
            var updatedUser = await service.GetUserAsync(userEntity.Id);

            updatedUser.FirstName = "Leonid";

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateUserAsync(updatedUser));

            Assert.Equal("New password must not match old.", exception.Message);
        }

    }
}