using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityTypeCollection : Collection<Type, EntityType>
    {
        private Ontology.Ontology _ontology { get; }

        public EntityTypeCollection(IEnumerable<Type> source, Ontology.Ontology ontology) : base(source)
        {
            _ontology = ontology;
        }

        protected override EntityType Select(Type arg)
        {
            return new EntityType(arg, _ontology);
        }
    }

    public class EntityType
    {
        public EntityType(Type source, Ontology.Ontology ontology)
        {
            Source = source;
            _ontology = ontology;
        }

        protected Type Source { get; }

        private Ontology.Ontology _ontology { get; }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLNonNullType] public string Title => Source.Title;

        [GraphQLNonNullType] public string Code => Source.Name;

        public bool IsAbstract => Source is Core.Ontology.EntityType et && et.IsAbstract; // todo

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

        protected IEntityAttribute CreateEntityAttribute(EmbeddingRelationType relationType)
        {
            return relationType.IsAttributeType
                ? (IEntityAttribute) new EntityAttributePrimitive(relationType)
                : new EntityAttributeRelation(relationType, _ontology);
        }
    }
}
