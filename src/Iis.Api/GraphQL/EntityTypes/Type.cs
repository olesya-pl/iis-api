using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.OntologySchema.DataTypes;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class EntityTypeCollection : Collection<INodeTypeLinked, EntityType>
    {
        public EntityTypeCollection(IEnumerable<INodeTypeLinked> source) : base(source)
        {
        }

        protected override EntityType Select(INodeTypeLinked arg)
        {
            return new EntityType(arg);
        }
    }

    public class EntityType
    {
        public EntityType(INodeTypeLinked source)
        {
            Source = source;
        }

        protected INodeTypeLinked Source { get; }

        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id => Source.Id;

        [GraphQLNonNullType] public string Title => Source.Title;

        [GraphQLNonNullType] public string Code => Source.Name;

        public bool IsAbstract => Source.IsAbstract;

        [GraphQLDeprecated("Entity can have multiple parents. You should use Parents property.")]
        public EntityType Parent =>
            Source.DirectParents.Select(p => new EntityType(p)).FirstOrDefault();

        [GraphQLNonNullType]
        public IEnumerable<EntityType> Parents => Source.DirectParents.Select(p => new EntityType(p));

        [GraphQLNonNullType]
        public IEnumerable<EntityType> Children => Source.GetAllDescendants().Concat(new[] { Source }).Select(child => new EntityType(child));


        [GraphQLType(typeof(NonNullType<ListType<NonNullType<EntityAttributeType>>>))]
        [GraphQLDescription("Get all type relations")]
        public IEnumerable<IEntityAttribute> GetAttributes()
        {
            var props = Source.AllProperties.Where(p => !p.IsComputed).OrderBy(a => a.CreatedAt);
            return props.Select(CreateEntityAttribute);
        }

        [GraphQLNonNullType]
        [GraphQLDescription("Entity contains unique values and needs dropdown tip on UI")]
        public bool HasUniqueValues => Source.HasUniqueValues;

        protected IEntityAttribute CreateEntityAttribute(INodeTypeLinked relationType)
        {
            return relationType.IsAttributeType
                ? (IEntityAttribute) new EntityAttributePrimitive(relationType)
                : new EntityAttributeRelation(relationType);
        }
    }
}
