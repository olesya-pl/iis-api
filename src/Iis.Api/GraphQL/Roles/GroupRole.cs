using System;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.Roles
{
    public class GroupRole
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }

        [GraphQLNonNullType]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}