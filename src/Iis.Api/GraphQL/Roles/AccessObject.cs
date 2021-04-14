using HotChocolate;
using HotChocolate.Types;
using System;
using System.Collections.Generic;

namespace IIS.Core.GraphQL.Roles
{
    public class AccessEntity
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        
        [GraphQLNonNullType]
        public string Kind { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }
        public IEnumerable<string> GrantedOperations { get; set; }
        public IEnumerable<string> AllowedOperations { get; set; }
    }
}
