using HotChocolate;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace Iis.Api.Ontology
{
    public class EventsAssociatedWithEntityResponse
    {
        [GraphQLType(typeof(JsonScalarType))]
        public JObject Event { get; set; }
    }
}
