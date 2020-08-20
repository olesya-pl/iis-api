using HotChocolate;
using Newtonsoft.Json.Linq;
using IIS.Core.GraphQL.Scalars;

namespace IIS.Core.GraphQL.Annotations
{

    public class Annotation : BaseAnnotation
    {
        [GraphQLType(typeof(JsonScalarType))]
        public JObject Content {get;set;}
    }
}