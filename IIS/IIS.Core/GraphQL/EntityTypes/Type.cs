using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityTypeCollection : Collection<Type, EntityType>
    {
        public EntityTypeCollection(IEnumerable<Type> source) : base(source)
        {
        }

        protected override EntityType Select(Type arg)
        {
            return new EntityType(arg);
        }
    }

    public class EntityType
    {
        public EntityType(Type source)
        {
            Source = source;
        }

        protected Type Source { get; }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLNonNullType] public string Title => Source.Title;

        [GraphQLNonNullType] public string Code => Source.Name;

        public bool IsAbstract => Source is Core.Ontology.EntityType et && et.IsAbstract; // todo

        [GraphQLDeprecated("Entity can have multiple parents. You should use Parents property.")]
        public EntityType Parent =>
            Source.DirectParents.Select(p => new EntityType(p)).FirstOrDefault();

        [GraphQLNonNullType]
        public IEnumerable<EntityType> Parents => Source.DirectParents.Select(p => new EntityType(p));

        [GraphQLType(typeof(NonNullType<ListType<NonNullType<EntityAttributeType>>>))]
        [GraphQLDescription("Get all type relations. Sort argument is deprecated.")]
        public IEnumerable<IEntityAttribute> GetAttributes(bool? sort = false)
        {
            var props = Source.AllProperties;
            if (sort == true)
                props = props.OrderBy(a => a.EmbeddingMeta.SortOrder);
            return props.Select(CreateEntityAttribute);
        }

        protected IEntityAttribute CreateEntityAttribute(EmbeddingRelationType relationType)
        {
            return relationType.IsAttributeType
                ? (IEntityAttribute) new EntityAttributePrimitive(relationType)
                : new EntityAttributeRelation(relationType);
        }
    }
}
