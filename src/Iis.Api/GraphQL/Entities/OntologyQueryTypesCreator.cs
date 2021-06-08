using HotChocolate.Types;
using Iis.Api.GraphQL.Entities;
using Iis.Domain;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.GraphQL.Entities.Resolvers;
using System;
using System.Linq;
using RelationType = IIS.Core.GraphQL.Entities.ObjectTypes.RelationType;

namespace IIS.Core.GraphQL.Entities
{
    public class OntologyQueryTypesCreator
    {
        private readonly TypeRepository _repository;

        public OntologyQueryTypesCreator(TypeRepository repository)
        {
            _repository = repository;
        }

        // ----- Object creation ----- //

        protected void OnObject(INodeTypeLinked type, IObjectTypeDescriptor d = null)
        {
            foreach (var parentInterface in type.AllParents.Select(_repository.GetOntologyType).OfType<InterfaceType>())
                d?.Interface(parentInterface);
            if (d == null) return;
            d.Name(OntologyObjectType.GetName(type));
            d.Interface(_repository.GetType<EntityInterface>());
            d.Field("id").Type<NonNullType<IdType>>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveId(ctx));
            d.Field("createdAt").Type<NonNullType<DateTimeType>>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveCreatedAt(ctx));
            d.Field("updatedAt").Type<NonNullType<DateTimeType>>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveUpdatedAt(ctx));
            d.Field("_relation").Type<RelationType>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveParentRelation(ctx));
            d.Field("coordinates").Type<ListType<ObjectType<GeoCoordinate>>>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveCoordinatesAsync(ctx));
            d.Field("createdByUser").Type<StringType>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveCreatedBy(ctx));
            d.Field("__iconName").Type<StringType>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveIconName(ctx));
            d.Field("__title").Type<StringType>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveTitle(ctx));
            d.Field("__accessLevel").Type<IntType>().Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveAccessLevel(ctx));
        }

        protected void OnRelation(INodeTypeLinked relationType, IObjectTypeDescriptor objectTypeDescriptor = null)
        {
            var type = GetType(relationType);
            if (objectTypeDescriptor == null) return;
            var fd = objectTypeDescriptor.Field(relationType.Name).Type(type.WrapOutputType(relationType));
            if (relationType.IsAttributeType)
            {
                if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                    fd.Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveMultipleAttributeRelation(ctx, relationType));
                else
                    fd.Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveAttributeRelation(ctx, relationType));
            }
            else
            {
                fd.Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveEntityRelation(ctx, relationType));
            }
        }

        private void OnRelation(INodeTypeLinked entityType, IGrouping<string, INodeTypeLinked> relationTypeGroup,
            IObjectTypeDescriptor objectTypeDescriptor = null)
        {
            if (relationTypeGroup.Count() == 1)
            {
                OnRelation(relationTypeGroup.Single(), objectTypeDescriptor);
            }
            else
            {
                var first = relationTypeGroup.First();
                var union = GetOutputUnionType(entityType, relationTypeGroup);
                objectTypeDescriptor?.Field(relationTypeGroup.Key).Type(union.WrapOutputType(first))
                    .ResolverNotImplemented();
            }
        }

        // ----- Interfaces creation ----- //

        protected void OnInterface(INodeTypeLinked type, IInterfaceTypeDescriptor d = null)
        {
            if (d == null) return;
            d.Name(OntologyInterfaceType.GetName(type));
            d.Field("id").Type<NonNullType<IdType>>();
            d.Field("createdAt").Type<NonNullType<DateTimeType>>();
            d.Field("updatedAt").Type<NonNullType<DateTimeType>>();
            d.Field("_relation").Type<RelationType>();
            d.Field("__title").Type<StringType>();
            d.ResolveAbstractType((ctx, obj) => ctx.Service<IOntologyQueryResolver>().ResolveAbstractType(ctx, obj));
        }

        protected void OnRelation(INodeTypeLinked relationType,
            IInterfaceTypeDescriptor interfaceTypeDescriptor = null)
        {
            var type = GetType(relationType);
            interfaceTypeDescriptor?.Field(relationType.Name).Type(type.WrapOutputType(relationType));
        }

        private void OnRelation(INodeTypeLinked entityType, IGrouping<string, INodeTypeLinked> relationTypeGroup,
            IInterfaceTypeDescriptor interfaceTypeDescriptor = null)
        {
            if (relationTypeGroup.Count() == 1)
            {
                OnRelation(relationTypeGroup.Single(), interfaceTypeDescriptor);
            }
            else
            {
                var first = relationTypeGroup.First();
                var union = GetOutputUnionType(entityType, relationTypeGroup);
                interfaceTypeDescriptor?.Field(relationTypeGroup.Key).Type(union.WrapOutputType(first));
            }
        }

        private OutputUnionType GetOutputUnionType(INodeTypeLinked entityType,
            IGrouping<string, INodeTypeLinked> relationTypeGroup)
        {
            if (relationTypeGroup.Any(r => !r.IsEntityType))
                throw new ArgumentException(
                    $"Found attributes with same relation name: {entityType.Name}.{relationTypeGroup.Key}");
            var graphTypes = relationTypeGroup.Where(r => r.IsEntityType)
                .Select(r => _repository.GetOntologyType(r.EntityType)).ToList();
            if (graphTypes.Any(t => !(t is ObjectType)))
                throw new ArgumentException(
                    $"Can not create union with non-object type {entityType.Name}.{relationTypeGroup.Key}");
            return _repository.GetOutputUnionType(entityType, relationTypeGroup.Key, graphTypes.OfType<ObjectType>());
        }

        // ----- Generic ----- //

        private IOutputType GetType(INodeTypeLinked relationType)
        {
            var type = relationType.IsAttributeType
                ? GetAttributeType(relationType)
                : relationType.IsEntityType
                    ? _repository.GetOntologyType(relationType.EntityType)
                    : throw new ArgumentException(nameof(relationType));
            return type;
        }

        private IOutputType GetAttributeType(INodeTypeLinked relationType)
        {
            var type = relationType.AttributeTypeModel;
            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                return _repository.GetMultipleOutputType(type.ScalarTypeEnum);
            return _repository.GetScalarOutputType(type.ScalarTypeEnum);
        }

        public IOntologyType NewOntologyType(INodeTypeLinked type)
        {
            if (type.IsAbstract)
            {
                Action<IInterfaceTypeDescriptor> configure = d =>
                {
                    OnInterface(type, d);
                    foreach (var attr in type.AllProperties.GroupBy(r => r.Name))
                        OnRelation(type, attr, d);
                };
                configure(null);
                return new OntologyInterfaceType(configure);
            }
            else
            {
                Action<IObjectTypeDescriptor> configure = d =>
                {
                    OnObject(type, d);
                    foreach (var attr in type.AllProperties.GroupBy(r => r.Name))
                        OnRelation(type, attr, d);
                };
                configure(null);
                return new OntologyObjectType(configure);
            }
        }
    }
}
