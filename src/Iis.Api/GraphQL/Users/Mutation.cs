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
using Iis.Interfaces.Users;
using System.Linq;

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

            var existingUser = userService.GetUser(user.Id);
            if (existingUser == null)
                throw new Exception($"User not found, id = {user.Id}");

            if (existingUser.Source != UserSource.Internal)
            {
                ValidateExternalUserInput(user, existingUser);
            }

            var domainUser = mapper.Map<Iis.Domain.Users.User>(user);

            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                domainUser.PasswordHash = userService.GetPasswordHashAsBase64String(user.Password);
            }

            var userId = await userService.UpdateUserAsync(domainUser);

            domainUser = await userService.GetUserAsync(userId);

            return mapper.Map<User>(domainUser);
        }

        private bool IsEqual(string s1, string s2) =>
            s1 == s2 || string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2);

        private void ValidateExternalUserInput(UserUpdateInput userInput, Iis.Domain.Users.User user)
        {
            if (!IsEqual(userInput.FirstName, user.FirstName))
                throw new ArgumentException("Cannot update FirstName for external user");

            if (!IsEqual(userInput.LastName, user.LastName))
                throw new ArgumentException("Cannot update LastName for external user");

            if (!IsEqual(userInput.Patronymic, user.Patronymic))
                throw new ArgumentException("Cannot update Patronymic for external user");

            if (!string.IsNullOrEmpty(userInput.Password))
                throw new ArgumentException("Cannot update password for external user");

            var oldRoles = user.Roles.OrderBy(r => r.Id).Select(r => r.Id);
            var newRoles = userInput.Roles.OrderBy(id => id);
            if (!Enumerable.SequenceEqual(oldRoles, newRoles))
                throw new ArgumentException("Cannot update Roles for external user");
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
