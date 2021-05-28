using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using Iis.DataModel;
using Iis.Services;
using Iis.DbLayer.Repositories;
using Iis.Services.Contracts.Interfaces;
using Iis.Utility;

namespace IIS.Core.GraphQL.Users
{
    public class Mutation
    {
        private IConfiguration _configuration;
        public Mutation(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<User> CreateUser(
            [Service] IUserService userService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] UserCreateInput user)
        {
            Validator.ValidateObject(user, new ValidationContext(user), true);

            var domainUser = mapper.Map<Iis.Domain.Users.User>(user);

            domainUser.PasswordHash = userService.GetPasswordHashAsBase64String(user.Password);

            var userId = await userService.CreateUserAsync(domainUser);
            
            domainUser = await userService.GetUserAsync(userId);

            return mapper.Map<User>(domainUser);
        }
        public async Task<User> UpdateUser(
            [Service] IUserService userService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] UserUpdateInput user)
        {
            Validator.ValidateObject(user, new ValidationContext(user), true);

            var domainUser = mapper.Map<Iis.Domain.Users.User>(user);

            if (!string.IsNullOrWhiteSpace(user.Password)) 
            {
                domainUser.PasswordHash = userService.GetPasswordHashAsBase64String(user.Password);
            }

            var userId = await userService.UpdateUserAsync(domainUser);

            domainUser = await userService.GetUserAsync(userId);

            return mapper.Map<User>(domainUser);
        }

        public async Task<User> DeleteUser([Service] OntologyContext context, [Service] IMapper mapper, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            //TODO: should not be able to delete yourself
            var dbUser = await context.Users.FindAsync(id);
            if (dbUser == null)
                throw new InvalidOperationException($"User with id = {id} not found");
            context.Users.Remove(dbUser);
            await context.SaveChangesAsync();

            return mapper.Map<User>(dbUser);
        }

        public async Task<User> AssignRole([Service] IUserService userService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid userId,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid roleId)
        {
            var user = await userService.AssignRole(userId, roleId);
            return mapper.Map<User>(user);
        }
        public async Task<User> RejectRole([Service] IUserService userService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid userId,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid roleId)
        {
            var user = await userService.RejectRole(userId, roleId);
            return mapper.Map<User>(user);
        }
    }
}
