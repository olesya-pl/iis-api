using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using Iis.Services;
using Iis.DbLayer.Repositories;
using Iis.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Users
{
    public class Query
    {
        public async Task<User> GetUser(
            [Service] IUserService userService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var user = await userService.GetUserAsync(id);

            return mapper.Map<User>(user);
        }
        public async Task<GraphQLCollection<User>> GetUsers(
            [Service] IUserService userService,
            [Service] IMapper mapper,
            [GraphQLNonNullType] PaginationInput pagination)
        {
            var usersResult = await userService.GetUsersAsync(pagination.Offset(), pagination.PageSize);

            var graphQLUsers = usersResult.Users
                                    .Select(user => mapper.Map<User>(user))
                                    .ToList();

            return new GraphQLCollection<User>(graphQLUsers, usersResult.TotalCount);
        }

        public async Task<List<User>> GetOperators(
            [Service] IUserService userService,
            [Service] IMapper mapper)
        {
            var operators = await userService.GetOperatorsAsync();

            return operators
                    .Select(user => mapper.Map<User>(user))
                    .ToList();

        }
    }
}
