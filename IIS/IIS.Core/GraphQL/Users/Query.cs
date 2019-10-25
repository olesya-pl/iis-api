using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.GraphQL.Users
{
    public class Query
    {
        private static UserSession Stub => new UserSession
        {
            User = new User { Username = "User", Id = Guid.Empty, Name = "John Doe" },
            Token =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJiOWFiODZiMC05ZjEwLTExZTktOGNjYy00ZGM4NDRjMWUzMjUiLCJpYXQiOjE1NjM4ODQ2MTMsImV4cCI6MTU2MzkyMDYxM30.7K3rP7jLZqTCgBnYYPKx3bbwc5udRgDZ1dhW7ONbLS4"
        };

        public async Task<User> GetUser([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
                throw new InvalidOperationException($"Cannot find user with id = {id}");

            return new User(user);
        }

        public async Task<GraphQLCollection<User>> GetUsers([Service] OntologyContext context, [GraphQLNonNullType] PaginationInput pagination)
        {
            var users = context.Users.GetPage(pagination).Select(dbUser => new User(dbUser));
            return new GraphQLCollection<User>(await users.ToListAsync(), await context.Users.CountAsync());
        }

        public UserSession GetUserSession([GraphQLNonNullType] string username, [GraphQLNonNullType] string password) => Stub;

        public UserSession RefreshToken() => Stub;
    }
}
