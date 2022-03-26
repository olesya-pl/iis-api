using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.Roles
{
    public class GroupRoles
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<GroupRole> Roles { get; set; }
    }
}