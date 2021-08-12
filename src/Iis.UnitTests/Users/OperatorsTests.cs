using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DataModel.Roles;
using Iis.DbLayer.Repositories;
using Iis.Services;
using Iis.Services.Contracts.Interfaces;
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
            _serviceProvider = Utils.GetServiceProvider();
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
            foreach (var user in data)
            {
                user.UserRoles.Add(new UserRoleEntity {
                    RoleId = RoleEntity.OperatorRoleId,
                    User = user
                });
                user.IsBlocked = false;
            }
            context.AddRange(data);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<IUserService>();
            var result = await sut.GetOperatorsAsync();

            //assert
            Assert.Equal(data.Count, result.Count);
            foreach (var assignee in result)
            {
                var dataItem = data.First(p => p.Id == assignee.Id);
                UserTestHelper.AssertUserEntityMappedToUserCorrectly(dataItem, assignee);
            }
        }

        [Theory, RecursiveAutoData]
        public async Task GetAvailableOperators_ReturnsUsersWithLessThan10UnprocessedFiles(
            UserEntity operator1,
            UserEntity operator2,
            MaterialEntity dummyMaterial,
            MaterialSignEntity materialSign)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            operator1.UserRoles.Add(new UserRoleEntity
            {
                RoleId = RoleEntity.OperatorRoleId,
                User = operator1
            });
            operator1.IsBlocked = false;

            operator2.UserRoles.Add(new UserRoleEntity
            {
                RoleId = RoleEntity.OperatorRoleId,
                User = operator2
            });
            operator2.IsBlocked = false;

            context.AddRange(new[] { operator1, operator2 });

            for (var i = 0; i < 20; i++)
            {
                dummyMaterial.AssigneeId = operator1.Id;
                dummyMaterial.Assignee = operator1;
                dummyMaterial.Id = new Guid();
                dummyMaterial.ParentId = null;
                dummyMaterial.Parent = null;
                context.Add(dummyMaterial);
                context.SaveChanges();
            }
            var toUpdate = context.Materials.OrderBy(p => p.Id).Skip(0).Take(10);
            foreach (var item in toUpdate)
            {
                item.AssigneeId = operator1.Id;
                item.Assignee = operator1;
            }
            context.SaveChanges();

            toUpdate = context.Materials.OrderBy(p => p.Id).Skip(10).Take(10);
            foreach (var item in toUpdate)
            {
                item.AssigneeId = operator2.Id;
                item.Assignee = operator2;
            }
            context.SaveChanges();

            materialSign.Id = MaterialEntity.ProcessingStatusNotProcessedSignId;
            toUpdate = context.Materials.OrderBy(p => p.Id).Skip(1).Take(19);
            foreach (var item in toUpdate)
            {
                item.ProcessedStatus = materialSign;
                item.ProcessedStatusSignId = materialSign.Id;
                context.SaveChanges();
            }

            //act
            var sut = _serviceProvider.GetRequiredService<IUserService>();
            var result = await sut.GetAvailableOperatorIdsAsync();

            //assert
            Assert.DoesNotContain(operator2.Id, result);
            Assert.Contains(operator1.Id, result);
        }

        [Theory, RecursiveAutoData]
        public async Task GetAvailableOperators_NoMaterialsAssigned_ReturnsAllUsers(
            List<UserEntity> operators,
            List<MaterialEntity> materials)
        {
            //arrange
            foreach (var material in materials)
            {
                material.Assignee = null;
                material.AssigneeId = null;
                material.ProcessedStatusSignId = null;
                material.ProcessedStatus = null;
            }

            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            foreach (var user in operators)
            {
                user.UserRoles.Add(new UserRoleEntity
                {
                    RoleId = RoleEntity.OperatorRoleId,
                    User = user
                });
                user.IsBlocked = false;
            }
            context.AddRange(operators);
            context.AddRange(materials);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<IUserService>();
            var result = await sut.GetAvailableOperatorIdsAsync();

            //assert
            Assert.Equal(operators.Count, result.Count);
        }
    }
}
