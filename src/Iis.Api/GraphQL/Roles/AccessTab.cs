using HotChocolate;
using HotChocolate.Types;
using System;

namespace IIS.Core.GraphQL.Roles
{
    public class AccessTab
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }

        [GraphQLNonNullType]
        public string Kind { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }
        public bool Visible { get; set; }
    }
}
