using HotChocolate;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.Roles
{
    public class AccessEntity
    {
        [GraphQLNonNullType]
        public string Kind { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }
        public IEnumerable<string> AllowedOperations { get; set; }
    }
}
