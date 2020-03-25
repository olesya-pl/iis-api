using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.Roles
{
    public class AccessTab
    {
        [GraphQLNonNullType]
        public string Kind { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }
    }
}
