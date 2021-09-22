using HotChocolate;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.Common
{
    public class IdTitle
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string NodeTypeName { get; set; }
    }
}
