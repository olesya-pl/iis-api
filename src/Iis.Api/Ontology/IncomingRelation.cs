using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace Iis.Api.Ontology
{
    public class IncomingRelation
    {
        public string RelationTypeName { get; set; }
        public string RelationTypeTitle { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid EntityId { get; set; }
        public string EntityTypeName { get; set; }
        public string __typeName => $"Entity{EntityTypeName}";
        [GraphQLType(typeof(JsonScalarType))]
        public JObject Entity { get; set; }
    }
}
