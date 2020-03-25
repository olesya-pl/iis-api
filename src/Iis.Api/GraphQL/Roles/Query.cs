using HotChocolate;
using Iis.DataModel;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.Roles
{
    public class Query
    {
        public async Task<GraphQLCollection<Role>> GetUsers([Service] OntologyContext context)
        {
            var users = context.Roles(pagination).Select(user => new User(user));
            return new GraphQLCollection<Role>(await users.ToListAsync(), await context.Users.CountAsync());
        }
    }
}
