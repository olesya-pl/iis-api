using System;
using HotChocolate;
using HotChocolate.Types;
using Iis.Domain.Materials;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialFeature
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Relation { get; set; }
        public string Value { get; set; }
        public MaterialFeatureNode Node { get; set; }
        public EntityTypeNames NodeType { get; set; }
    }

    public class MaterialFeatureNode
    {
        public string Id { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid NodeTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
