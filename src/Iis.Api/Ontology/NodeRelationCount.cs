using System;
using HotChocolate;
using HotChocolate.Types;


namespace Iis.Api.Ontology
{
    public class NodeRelationCount
    {
        [GraphQLType(typeof(IdType))]
        public Guid NodeId { get; set; }
        public int RelationsCount { get; set; }
        public int EventsCount { get; set; }
        public int MaterialsCount { get; set; }
    }
}
