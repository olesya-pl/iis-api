using System;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.Roles
{
    public class Group
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
