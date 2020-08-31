using System;
using HotChocolate;
using HotChocolate.Types;
using Newtonsoft.Json.Linq;
using IIS.Core.GraphQL.Scalars;

namespace IIS.Core.GraphQL.Annotations
{

    public class Annotation
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }

        [GraphQLType(typeof(JsonScalarType))]
        public JObject Content {get;set;}
    }
}