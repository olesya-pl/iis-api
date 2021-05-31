using System;
using HotChocolate;
using HotChocolate.Types;
using Newtonsoft.Json.Linq;
using IIS.Core.GraphQL.Scalars;

namespace Iis.Api.GraphQL.Graph
{
    public class GraphNode
    {
        [GraphQLType(typeof(IdType))]
        public Guid Id { get; set; }
        [GraphQLType(typeof(JsonScalarType))]
        public JObject Extra { get; set; }
    }
}