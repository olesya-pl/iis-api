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

namespace IIS.Core.GraphQL.Users
{
    public class Mutation
    {
        private IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Mutation(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> CreateUser([Service] OntologyContext context, [GraphQLNonNullType] UserInput data)
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);
            if (context.Users.Any(u => u.Username == data.Username))
                throw new InvalidOperationException($"User {data.Username} already exists");

            var user = new Iis.DataModel.UserEntity
            {
                Id           = Guid.NewGuid(),
                IsBlocked    = data.IsBlocked,
                Name         = data.Name,
                Username     = data.Username,
                PasswordHash = _configuration.GetPasswordHashAsBase64String(data.Password)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return new User(user);
        }

        public async Task<User> UpdateUser([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] string id, [GraphQLNonNullType] UserInput data)
        {
            //TODO: should not be able to block yourself
            Validator.ValidateObject(data, new ValidationContext(data), true);
            var user = await context.Users.FindAsync(Guid.Parse(id));
            if (user == null)
                throw new InvalidOperationException($"Cannot find user with id = {id}");

            user.IsBlocked    = data.IsBlocked;
            user.Name         = data.Name;
            user.PasswordHash = _configuration.GetPasswordHashAsBase64String(data.Password);

            await context.SaveChangesAsync();

            return new User(user);
        }

        public async Task<User> DeleteUser([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            //TODO: should not be able to delete yourself
            var dbUser = await context.Users.FindAsync(id);
            if (dbUser == null)
                throw new InvalidOperationException($"User with id = {id} not found");
            context.Users.Remove(dbUser);
            await context.SaveChangesAsync();

            return new User(dbUser);
        }
    }
}
