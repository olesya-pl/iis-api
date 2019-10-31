using HotChocolate;
using HotChocolate.Types;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<User> CreateUser([Service] OntologyContext context, [GraphQLNonNullType] User user)
        {
            Validator.ValidateObject(user, new ValidationContext(user), true);
            if (context.Users.Any(u => u.UserName == user.Username))
                throw new InvalidOperationException($"User {user.Username} already exists");

            var userId = Guid.NewGuid();
            context.Users.Add(new Core.Users.EntityFramework.User
            {
                Id           = userId,
                IsBlocked    = user.IsBlocked,
                Name         = user.Name,
                UserName     = user.Username,
                PasswordHash = _configuration.GetPasswordHashAsBase64String(user.Password)
            });
            await context.SaveChangesAsync();
            user.Id = userId;
            return user;
        }

        public async Task<User> UpdateUser([Service] OntologyContext context, [GraphQLNonNullType] UserUpdateInput user)
        {
            //ToDo: !!! (не можна видаляти і блокувати самого себе)
            Validator.ValidateObject(user, new ValidationContext(user), true);
            var userInDb = context.Users.Find(user.Id);
            if (userInDb == null)
                throw new InvalidOperationException($"Cannot find user with id = {user.Id}");

            userInDb.IsBlocked    = user.IsBlocked;
            userInDb.Name         = user.Name;
            userInDb.PasswordHash = _configuration.GetPasswordHashAsBase64String(user.Password);

            await context.SaveChangesAsync();

            return new User(user, user.Password, userInDb.UserName);
        }

        public async Task<User> DeleteUser([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            //ToDo: !!! (не можна видаляти і блокувати самого себе)
            var dbUser = await context.Users.FindAsync(id);
            if (dbUser == null)
                throw new InvalidOperationException($"User with id = {id} not found");
            context.Users.Remove(dbUser);
            await context.SaveChangesAsync();

            return new User(dbUser);
        }
    }
}
