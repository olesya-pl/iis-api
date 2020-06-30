using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology;
using Iis.Domain;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityTypeCollection : Collection<NodeType, EntityType>
    {
        private OntologyModel _ontology { get; }

        public EntityTypeCollection(IEnumerable<NodeType> source, OntologyModel ontology) : base(source)
        {
            _ontology = ontology;
        }

        protected override EntityType Select(NodeType arg)
        {
            return new EntityType(arg, _ontology);
        }
    }

    public class EntityType
    {
        public EntityType(NodeType source, OntologyModel ontology)
        {
            Source = source;
            _ontology = ontology;
        }

        protected NodeType Source { get; }

        private OntologyModel _ontology { get; }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLNonNullType] public string Title => Source.Title;

        [GraphQLNonNullType] public string Code => Source.Name;

        public bool IsAbstract => Source is Iis.Domain.EntityType et && et.IsAbstract; // todo

        [GraphQLDeprecated("Entity can have multiple parents. You should use Parents property.")]
        public EntityType Parent =>
            Source.DirectParents.Select(p => new EntityType(p, _ontology)).FirstOrDefault();

        [GraphQLNonNullType]
        public IEnumerable<EntityType> Parents => Source.DirectParents.Select(p => new EntityType(p, _ontology));

        [GraphQLNonNullType]
        public IEnumerable<EntityType> Children => _ontology.GetChildTypes(Source).Select(child => new EntityType(child, _ontology));


        [GraphQLType(typeof(NonNullType<ListType<NonNullType<EntityAttributeType>>>))]
        [GraphQLDescription("Get all type relations")]
        public IEnumerable<IEntityAttribute> GetAttributes()
        {
            var props = Source.AllProperties.OrderBy(a => a.CreatedAt);
            return props.Select(CreateEntityAttribute);
        }

        [GraphQLNonNullType]
        [GraphQLDescription("Entity contains unique values and needs dropdown tip on UI")]
        public bool HasUniqueValues => Source.HasUniqueValues;

        protected IEntityAttribute CreateEntityAttribute(EmbeddingRelationType relationType)
        {
            return relationType.IsAttributeType
                ? (IEntityAttribute) new EntityAttributePrimitive(relationType)
                : new EntityAttributeRelation(relationType, _ontology);
        }
    }
}
