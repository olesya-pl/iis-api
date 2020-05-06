using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;

using Iis.DataModel;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Roles;

namespace IIS.Core.GraphQL.Users
{
    public class Query
    {
        public async Task<User2> GetUser2(
            [Service] OntologyContext context, 
            [Service] IMapper mapper, 
            [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            return new User2
            {
                Id = id,
                FirstName = "Александр",
                LastName = "Подервянский ",
                Patronymic = "Сергеевич",
                UserName = "artist",
                Comment = "no comments",
                Roles = new List<Role>{ new Role{ Id = System.Guid.NewGuid(), Name = "Администратор"}}
            };
        }
        public async Task<GraphQLCollection<User2>> GetUsers2(
            [Service] OntologyContext context, 
            [Service] IMapper mapper, 
            [GraphQLNonNullType] PaginationInput pagination)
        {
            return new GraphQLCollection<User2>( 
                new List<User2>{
                    new User2
                    {
                        Id = System.Guid.NewGuid(),
                        FirstName = "Александр",
                        LastName = "Подервянский ",
                        Patronymic = "Сергеевич",
                        UserName = "artist",
                        Comment = "no comments",
                        Roles = new List<Role>{ new Role{ Id = System.Guid.NewGuid(), Name = "Администратор"}}
                    }
                }, 1);
        }

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
