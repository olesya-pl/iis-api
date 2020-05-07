using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

using Iis.Roles;
using Iis.DataModel;
using DomainRoles = Iis.Roles;

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
            [Service] UserService userService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] UserCreateInput user)
        {
            Validator.ValidateObject(user, new ValidationContext(user), true);

            var domainUser = mapper.Map<DomainRoles.User>(user);

            domainUser.PasswordHash = _configuration.GetPasswordHashAsBase64String(user.Password);

            var userId = await userService.CreateUserAsync(domainUser);
            
            domainUser = await userService.GetUserAsync(userId);

            return mapper.Map<User>(domainUser);
        }
        public async Task<User> UpdateUser(
            [Service] UserService userService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] UserUpdateInput user)
        {
            var domainUser = mapper.Map<DomainRoles.User>(user);

            domainUser.PasswordHash = _configuration.GetPasswordHashAsBase64String(user.Password);

            await userService.UpdateUserAsync(domainUser);

            domainUser = await userService.GetUserAsync(new Guid("d86f8961-817b-4494-b0a8-330c037b5053"));

            return mapper.Map<User>(domainUser);
        }

        // public async Task<User> UpdateUser(
        //     [Service] OntologyContext context, 
        //     [Service] IMapper mapper, 
        //     [GraphQLType(typeof(NonNullType<IdType>))] string id, 
        //     [GraphQLNonNullType] UserInput data)
        // {
        //     //TODO: should not be able to block yourself
        //     Validator.ValidateObject(data, new System.ComponentModel.DataAnnotations.ValidationContext(data), true);
        //     var user = await context.Users.FindAsync(Guid.Parse(id));
        //     if (user == null)
        //         throw new InvalidOperationException($"Cannot find user with id = {id}");

        //     user.IsBlocked    = data.IsBlocked.GetValueOrDefault();
        //     user.Name         = data.Name;
        //     user.PasswordHash = _configuration.GetPasswordHashAsBase64String(data.Password);

        //     await context.SaveChangesAsync();

        //     return mapper.Map<User>(user);
        // }

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
    }
}
