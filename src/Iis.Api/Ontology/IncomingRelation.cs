using System;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.Ontology
{
    public class IncomingRelation
    {
        public string RelationTypeName { get; set; }
        public string RelationTypeTitle { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid EntityId { get; set; }
        public string EntityTypeName { get; set; }
    }
}
