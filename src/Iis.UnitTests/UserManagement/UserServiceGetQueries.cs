using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Iis.Roles;
using Iis.DataModel;
using Iis.DataModel.Roles;

namespace Iis.UnitTests.UserManagement
{
    public class UserServiceGetQueries : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public UserServiceGetQueries()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
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
        }

        [Fact(DisplayName = "No such user with exception")]
        public void GetUserById_NoSuchUser()
        {
            var service = _serviceProvider.GetRequiredService<UserService>();

            //act
            Exception exception = Assert.Throws<ArgumentException>(() => service.GetUserAsync(Guid.Empty).GetAwaiter().GetResult());

            //assert
            Assert.Equal($"User does not exist for id = {Guid.Empty}", exception.Message);
        }

        [Theory(DisplayName = "Get User by Id"), RecursiveAutoData]
        public void GetUserById(
            List<AccessObjectEntity> existingAccesses,
            RoleEntity roleEntity,
            UserEntity userEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<UserService>();

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

            //act
            var user = service.GetUserAsync(userEntity.Id)
                                    .GetAwaiter().GetResult();

            //assert
            Assert.Equal(userEntity.Id, user.Id);
            //TODO: this asserts should be uncommented after changes in Entity
            //Assert.Equal(userEntity., user.LastName);
            //Assert.Equal(userEntity., user.FirstName);
            //Assert.Equal(userEntity., user.Patronymic);
            Assert.Equal(userEntity.Username, user.UserName);
            Assert.Equal(userEntity.PasswordHash, user.PasswordHash);
            Assert.Equal(userEntity.IsBlocked, user.IsBlocked);
            Assert.Single(user.Roles);
            Assert.Equal(roleEntity.Id, user.Roles.FirstOrDefault().Id);
        }

        [Theory(DisplayName ="Get User list (size:1)"), RecursiveAutoData]
        public void GetUsersSize1(
            List<AccessObjectEntity> existingAccesses,
            List<UserEntity> userEntities,
            RoleEntity roleEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<UserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            roleEntity.UserRoles = null;
            userEntities.ForEach(e => e.UserRoles = null);
            existingAccesses.ForEach(e => e.RoleAccessEntities = null);

            context.Roles.Add(roleEntity);
            context.Users.AddRange(userEntities);
            context.UserRoles.AddRange(userEntities.Select(userEntity => new UserRoleEntity { Id = Guid.NewGuid(), UserId = userEntity.Id, RoleId = roleEntity.Id }));
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
            var usersResult = service.GetUsersAsync(0, 1)
                                .GetAwaiter().GetResult();

            //assert
            Assert.Equal(userEntities.Count, usersResult.TotalCount);
            Assert.Single(usersResult.Users);
        }

        [Theory(DisplayName = "Get User list (size:10)"), RecursiveAutoData]
        public void GetUsersSize10(
            List<AccessObjectEntity> existingAccesses,
            List<UserEntity> userEntities,
            RoleEntity roleEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<UserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            roleEntity.UserRoles = null;
            userEntities.ForEach(e => e.UserRoles = null);
            existingAccesses.ForEach(e => e.RoleAccessEntities = null);

            context.Roles.Add(roleEntity);
            context.Users.AddRange(userEntities);
            context.UserRoles.AddRange(userEntities.Select(userEntity => new UserRoleEntity { Id = Guid.NewGuid(), UserId = userEntity.Id, RoleId = roleEntity.Id }));
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
            var usersResult = service.GetUsersAsync(0, 10)
                                .GetAwaiter().GetResult();

            //assert
            Assert.Equal(userEntities.Count, usersResult.TotalCount);
            Assert.Equal(userEntities.Count, usersResult.Users.Count());
        }
    }
}