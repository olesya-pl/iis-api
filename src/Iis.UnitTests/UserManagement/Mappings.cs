using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AutoMapper;
using Iis.Services.Contracts;
using GraphUsers = IIS.Core.GraphQL.Users;

namespace Iis.UnitTests.UserManagement
{
    public class Mappings : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        public Mappings()
        {
            _serviceProvider = Utils.GetServiceProvider();
            _mapper = _serviceProvider.GetRequiredService<IMapper>();
        }
        public void Dispose()
        {
            _serviceProvider.Dispose();
        }

        [Theory(DisplayName = "Checks Automapper mapping: GraphQL UserCreateInput -> Domain User")]
        [RecursiveAutoData]
        public void MapUserCreateInputToUser(GraphUsers.UserCreateInput input)
        {
            var user = _mapper.Map<User>(input);

            Assert.NotEqual(user.Id, Guid.Empty);
            Assert.Equal(input.LastName, user.LastName);
            Assert.Equal(input.FirstName, user.FirstName);
            Assert.Equal(input.Patronymic, user.Patronymic);
            Assert.Equal(input.Comment, user.Comment);
            Assert.Equal(input.UserName, user.UserName);
            Assert.Equal(input.UserNameActiveDirectory, user.UserNameActiveDirectory);
            
            Assert.Equal(input.Roles.Count(), user.Roles.Count());
            Assert.Equal(input.Roles, user.Roles.Select(r => r.Id));
        }
        [Theory(DisplayName = "Checks Automapper mapping: GraphQL UserUpdateInput -> Domain User")]
        [RecursiveAutoData]
        public void MapUserUpdateInputToUser(GraphUsers.UserUpdateInput input)
        {
            var user = _mapper.Map<User>(input);

            Assert.Equal(input.Id, user.Id);
            Assert.Equal(input.LastName, user.LastName);
            Assert.Equal(input.FirstName, user.FirstName);
            Assert.Equal(input.Patronymic, user.Patronymic);
            Assert.Equal(input.Comment, user.Comment);
            Assert.Null(user.UserName);
            Assert.Equal(input.UserNameActiveDirectory, user.UserNameActiveDirectory);
            
            Assert.Equal(input.Roles.Count(), user.Roles.Count());
            Assert.Equal(input.Roles, user.Roles.Select(r => r.Id));
        }

        [Theory(DisplayName = "Checks Automapper mapping: Domail User -> GraphQL User")]
        [RecursiveAutoData]
        public void MapUserToUserOutput(User domainUser)
        {
            var graphQlUser = _mapper.Map<GraphUsers.User>(domainUser);

            Assert.Equal(domainUser.Id, graphQlUser.Id);
            Assert.Equal(domainUser.LastName, graphQlUser.LastName);
            Assert.Equal(domainUser.FirstName, graphQlUser.FirstName);
            Assert.Equal(domainUser.Patronymic, graphQlUser.Patronymic);
            Assert.Equal(domainUser.Comment, graphQlUser.Comment);
            Assert.Equal(domainUser.UserName, graphQlUser.UserName);
            Assert.Equal(domainUser.UserNameActiveDirectory, graphQlUser.UserNameActiveDirectory);
            Assert.Equal(domainUser.IsBlocked, graphQlUser.IsBlocked);
            Assert.Equal(domainUser.IsAdmin, graphQlUser.IsAdmin);

            Assert.Equal(domainUser.Roles.Count(), graphQlUser.Roles.Count());
            Assert.Equal(domainUser.Roles.Select(r => r.Id), graphQlUser.Roles.Select(r => r.Id));
        }
    }
}