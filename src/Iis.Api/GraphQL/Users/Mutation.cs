using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using AutoMapper;
using Iis.Roles;

namespace IIS.Core.GraphQL.Users
{
    public class Mutation
    {
        private IConfiguration _configuration;
        public Mutation(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<User2> CreateUser2(
            [Service] OntologyContext context,
            [Service] IMapper mapper,
            [GraphQLNonNullType] User2Input user)
        {
            return new User2
            {
                Id = System.Guid.NewGuid(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Patronymic = user.Patronymic,
                Username = user.UserName,
                Comment = user.Comment
            };
        }
        public async Task<User2> UpdateUser2(
            [Service] OntologyContext context,
            [Service] IMapper mapper,
            [GraphQLNonNullType] User2Input user)
        {
            return new User2
            {
                Id = user.Id.HasValue ? user.Id.Value : System.Guid.NewGuid(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Patronymic = user.Patronymic,
                Username = user.UserName,
                Comment = user.Comment
            };
        }

        public async Task<User> CreateUser([Service] OntologyContext context, [Service] IMapper mapper, [GraphQLNonNullType] UserInput data)
        {
            Validator.ValidateObject(data, new System.ComponentModel.DataAnnotations.ValidationContext(data), true);
            if (context.Users.Any(u => u.Username == data.Username))
                throw new InvalidOperationException($"User {data.Username} already exists");

            var user = new Iis.DataModel.UserEntity
            {
                Id           = Guid.NewGuid(),
                IsBlocked    = data.IsBlocked.GetValueOrDefault(),
                Name         = data.Name,
                Username     = data.Username,
                PasswordHash = _configuration.GetPasswordHashAsBase64String(data.Password)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return mapper.Map<User>(user);
        }

        public async Task<User> UpdateUser(
            [Service] OntologyContext context, 
            [Service] IMapper mapper, 
            [GraphQLType(typeof(NonNullType<IdType>))] string id, 
            [GraphQLNonNullType] UserInput data)
        {
            //TODO: should not be able to block yourself
            Validator.ValidateObject(data, new System.ComponentModel.DataAnnotations.ValidationContext(data), true);
            var user = await context.Users.FindAsync(Guid.Parse(id));
            if (user == null)
                throw new InvalidOperationException($"Cannot find user with id = {id}");

            user.IsBlocked    = data.IsBlocked.GetValueOrDefault();
            user.Name         = data.Name;
            user.PasswordHash = _configuration.GetPasswordHashAsBase64String(data.Password);

            await context.SaveChangesAsync();

            return mapper.Map<User>(user);
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

        public async Task<User> AssignRole([Service] RoleService roleSaver,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid userId,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid roleId,
            [GraphQLNonNullType] bool isActive)
        {
            var user = await roleSaver.AssignRole(userId, roleId, isActive);
            return mapper.Map<User>(user);
        }

    }
}
