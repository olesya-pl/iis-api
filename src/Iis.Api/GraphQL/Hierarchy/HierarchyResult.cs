using HotChocolate;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace Iis.Api.GraphQL.Hierarchy
{
    public class HierarchyResult
    {
        [GraphQLType(typeof(JsonScalarType))]
        public JObject Data { get; set; }
    }
}
