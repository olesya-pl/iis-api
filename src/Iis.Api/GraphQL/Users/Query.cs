using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace IIS.Core.GraphQL.Users
{
    public class Query
    {
        public async Task<User> GetUser([Service] OntologyContext context, [Service] IMapper mapper, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
                throw new InvalidOperationException($"Cannot find user with id \"{id}\"");

            return mapper.Map<User>(user);
        }

        public async Task<GraphQLCollection<User>> GetUsers([Service] OntologyContext context, [Service] IMapper mapper, [GraphQLNonNullType] PaginationInput pagination)
        {
            var users = context.Users.GetPage(pagination).Select(user => mapper.Map<User>(user));
            return new GraphQLCollection<User>(await users.ToListAsync(), await context.Users.CountAsync());
        }
    }
}
