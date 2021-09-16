using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AutoFixture;
using Xunit;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.UnitTests.TestHelpers;
using Iis.Services.Contracts.Enums;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
namespace Iis.UnitTests.UserManagement
{
    public class UserServiceGetQueries : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public UserServiceGetQueries()
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

        [Fact(DisplayName = "No such user with exception")]
        public async Task GetUserById_NoSuchUser()
        {
            var service = _serviceProvider.GetRequiredService<IUserService>();

            //act
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetUserAsync(Guid.Empty));

            //assert
            Assert.Equal($"User does not exist for id = {Guid.Empty}", exception.Message);
        }

        [Theory(DisplayName = "Get User by Id"), RecursiveAutoData]
        public async Task GetUserById(
            List<AccessObjectEntity> existingAccesses,
            RoleEntity roleEntity,
            UserEntity userEntity)
        {
            // arrange: begin
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

            //act
            var user = await service.GetUserAsync(userEntity.Id);

            //assert
            UserTestHelper.AssertUserEntityMappedToUserCorrectly(userEntity, user);
            Assert.Single(user.Roles);
            Assert.Equal(roleEntity.Id, user.Roles.FirstOrDefault().Id);
        }

        [Theory(DisplayName ="Get User list (size:1)"), RecursiveAutoData]
        public async Task GetUsersSize1(
            List<AccessObjectEntity> existingAccesses,
            List<UserEntity> userEntities,
            RoleEntity roleEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

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

            var page = new PaginationParams(1, 1);
            // arrange: end

            //act
            var usersResult = await service.GetUsersByStatusAsync(page, null, UserStatusType.All);

            //assert
            Assert.Equal(userEntities.Count, usersResult.TotalCount);
            Assert.Single(usersResult.Users);
        }

        [Theory(DisplayName = "Get User list (size:2)"), RecursiveAutoData]
        public async Task GetUsersSize2(
            List<AccessObjectEntity> existingAccesses,
            List<UserEntity> userEntities,
            RoleEntity roleEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

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

            var page = new PaginationParams(1, 2);

            // arrange: end

            //act
            var usersResult = await service.GetUsersByStatusAsync(page, null, UserStatusType.All);

            //assert
            Assert.Equal(userEntities.Count, usersResult.TotalCount);
            Assert.Equal(2, usersResult.Users.Count());
        }

        [Theory(DisplayName = "Get User match by full name")]
        [InlineData("ome nam")]
        [InlineData("Some ")]
        [InlineData("e name")]
        [InlineData("Some name")]
        public async Task GetUsersMatchByFullName(string suggestion)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            var fixture = new RecursiveAutoDataAttribute().Fixture;
            var existingAccesses = fixture.Create<List<AccessObjectEntity>>();
            var userEntities = fixture.Create<List<UserEntity>>();
            var roleEntity = fixture.Create<RoleEntity>();
            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            roleEntity.UserRoles = null;
            userEntities.ForEach(e => e.UserRoles = null);
            existingAccesses.ForEach(e => e.RoleAccessEntities = null);
            var firstUser = userEntities.First();

            firstUser.Name = "Some name";

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

            var page = new PaginationParams(1, 20);
            // arrange: end

            //act
            var usersResult = await service.GetUsersByStatusAsync(page, suggestion, UserStatusType.All);

            //assert
            Assert.Equal(1, usersResult.TotalCount);
            Assert.Equal(firstUser.Id, usersResult.Users.First().Id);
        }

        [Theory(DisplayName = "Get User match by user name")]
        [InlineData("ome nam")]
        [InlineData("Some ")]
        [InlineData("e name")]
        [InlineData("Some name")]
        public async Task GetUsersMatchByUserName(string suggestion)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            var fixture = new RecursiveAutoDataAttribute().Fixture;
            var existingAccesses = fixture.Create<List<AccessObjectEntity>>();
            var userEntities = fixture.Create<List<UserEntity>>();
            var roleEntity = fixture.Create<RoleEntity>();
            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            roleEntity.UserRoles = null;
            userEntities.ForEach(e => e.UserRoles = null);
            existingAccesses.ForEach(e => e.RoleAccessEntities = null);
            var firstUser = userEntities.First();

            firstUser.Username = "Some name";

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

            var page = new PaginationParams(1, 20);
            // arrange: end

            //act
            var usersResult = await service.GetUsersByStatusAsync(page, suggestion, UserStatusType.All);

            //assert
            Assert.Equal(1, usersResult.TotalCount);
            Assert.Equal(firstUser.Id, usersResult.Users.First().Id);
        }

        [Theory(DisplayName = "Get Blocked User list"), RecursiveAutoData]
        public async Task GetBlockedUserListSize2(
            List<AccessObjectEntity> existingAccesses,
            List<UserEntity> userEntities,
            RoleEntity roleEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            roleEntity.UserRoles = null;
            userEntities.ForEach(e => e.UserRoles = null);
            userEntities.ForEach(e => e.IsBlocked = true);
            userEntities.First().IsBlocked = false;

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

            var page = new PaginationParams(1, 1);

            // arrange: end

            //act
            var usersResult = await service.GetUsersByStatusAsync(page, null, UserStatusType.Blocked);

            //assert
            Assert.Equal(2, usersResult.TotalCount);
            Assert.Equal(1, usersResult.Users.Count());
        }

        [Theory(DisplayName = "Get Active User list"), RecursiveAutoData]
        public async Task GetActiveUserListSize2(
            List<AccessObjectEntity> existingAccesses,
            List<UserEntity> userEntities,
            RoleEntity roleEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            roleEntity.UserRoles = null;
            userEntities.ForEach(e => e.UserRoles = null);
            userEntities.ForEach(e => e.IsBlocked = false);
            userEntities.First().IsBlocked = true;

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

            var page = new PaginationParams(1, 1);

            // arrange: end

            //act
            var usersResult = await service.GetUsersByStatusAsync(page, null, UserStatusType.Active);

            //assert
            Assert.Equal(2, usersResult.TotalCount);
            Assert.Equal(1, usersResult.Users.Count());
        }

        [Theory(DisplayName = "Get Operators list"), RecursiveAutoData]
        public async Task GetOperatorsList(
            List<AccessObjectEntity> existingAccesses,
            List<UserEntity> userEntities,
            RoleEntity roleEntity)
        {
            // arrange:begin
            var service = _serviceProvider.GetRequiredService<IUserService>();

            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            roleEntity.RoleAccessEntities = new List<RoleAccessEntity>();

            roleEntity.UserRoles = null;

            var operatorRoleEntity = new RoleEntity
            {
                Id = RoleEntity.OperatorRoleId,
                UserRoles = null,
                RoleAccessEntities = new List<RoleAccessEntity>()
            };

            userEntities.ForEach(e => e.UserRoles = null);

            existingAccesses.ForEach(e => e.RoleAccessEntities = null);

            var userRolesEntitiesList = userEntities
                .Select(userEntity => new UserRoleEntity { Id = Guid.NewGuid(), UserId = userEntity.Id, RoleId = roleEntity.Id })
                .ToArray();

            userRolesEntitiesList.First().RoleId = operatorRoleEntity.Id;

            context.Roles.Add(roleEntity);
            context.Roles.Add(operatorRoleEntity);
            context.Users.AddRange(userEntities);
            context.UserRoles.AddRange(userRolesEntitiesList);
            context.AccessObjects.AddRange(existingAccesses);
            context.SaveChanges();
            foreach (var access in existingAccesses)
            {
                context.RoleAccess.Add(new RoleAccessEntity
                {
                    AccessObjectId = access.Id,
                    RoleId = roleEntity.Id
                });
                context.RoleAccess.Add(new RoleAccessEntity
                {
                    AccessObjectId = access.Id,
                    RoleId = operatorRoleEntity.Id
                });
            }
            context.SaveChanges();

            var page = new PaginationParams(1, 10);

            // arrange: end

            //act
            var operatorList = await service.GetOperatorsAsync();

            //assert
            Assert.Equal(1, operatorList.Count);
        }
     }
}