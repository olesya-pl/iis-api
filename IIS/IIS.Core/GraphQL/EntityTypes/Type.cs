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
        public EntityTypeCollection(IEnumerable<Type> source) : base(source){}

        protected override EntityType Select(Type arg) => new EntityType(arg);
    }

    public class EntityType
    {
        protected Type Source { get; }

        public EntityType(Type source)
        {
            Source = source;
        }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLNonNullType]
        public string Title => Source.Title;

        [GraphQLNonNullType]
        public string Code => Source.Name;

        public bool IsAbstract => Source is IIS.Core.Ontology.EntityType et && et.IsAbstract; // todo

        [GraphQLType(typeof(NonNullType<ListType<NonNullType<EntityAttributeType>>>))]
        [GraphQLDescription("Get all type relations. Sort argument is deprecated.")]
        public IEnumerable<IEntityAttribute> GetAttributes(bool? sort = false)
        {
            var props = Source.AllProperties;
            if (sort == true)
                props = props.OrderBy(a => a.CreateMeta().SortOrder);
            return props.Select(CreateEntityAttribute);
        }

        [GraphQLDeprecated("Entity can have multiple parents. You should use Parents property.")]
        public EntityType Parent =>
            Source.DirectParents.Select(p => new EntityType(p)).FirstOrDefault();

        [GraphQLNonNullType]
        public IEnumerable<EntityType> Parents => Source.DirectParents.Select(p => new EntityType(p));

        protected IEntityAttribute CreateEntityAttribute(EmbeddingRelationType relationType) =>
            relationType.IsAttributeType
                ? (IEntityAttribute) new EntityAttributePrimitive(relationType)
                : new EntityAttributeRelation(relationType);
    }
}
