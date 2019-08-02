using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Language;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;
using Humanizer;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using RelationType = IIS.Core.GraphQL.Entities.RelationType;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class ReadQueryTypeCreator : ITypeFieldPopulator
    {
        private readonly IGraphQlTypeRepository _typeRepository;
        private readonly GraphQlTypeCreator _creator;

        public ReadQueryTypeCreator(IGraphQlTypeRepository typeRepository, GraphQlTypeCreator creator)
        {
            _typeRepository = typeRepository;
            _creator = creator;
        }

        // ----- Object creation ----- //

        protected void OnObject(EntityType type, IObjectTypeDescriptor d = null)
        {
            foreach (var parentInterface in type.AllParents.Select(_creator.GetOntologyType).OfType<InterfaceType>())
                d?.Interface(parentInterface);
            if (d == null) return;
            d.Name(type.Name + "Entity");
            d.Interface(_creator.GetType<EntityInterface>());
            d.Field("id").Type<NonNullType<IdType>>().Resolver(ctx => Resolvers.ResolveId(ctx));
            d.Field("createdAt").Type<NonNullType<DateTimeType>>().ResolverNotImplemented();
            d.Field("updatedAt").Type<NonNullType<DateTimeType>>().ResolverNotImplemented();
            d.Field("_relation").Type<RelationType>().Resolver(ctx => Resolvers.ResolveParentRelation(ctx));
        }

        protected void OnRelation(EmbeddingRelationType relationType, IObjectTypeDescriptor objectTypeDescriptor = null)
        {
            var type = GetType(relationType);
            if (objectTypeDescriptor == null) return;
            var fd = objectTypeDescriptor.Field(relationType.GetFieldName()).Type(type.WrapOutputType(relationType));
            if (relationType.IsAttributeType)
                fd.Resolver(ctx => Resolvers.ResolveAttributeRelation(ctx, relationType));
            else
                fd.Resolver(ctx => Resolvers.ResolveEntityRelation(ctx, relationType));
            // todo: Move inversed relations to domain
            var meta = relationType.CreateMeta() as MetaForEntityTypeRelation;
            var inversed = meta?.Inversed;
            if (inversed != null)
            {
                objectTypeDescriptor.Field(inversed.Code ?? relationType.GetFieldName() + "Inversed").Type(type)
                    .Description("Inversed relation resolvers and wrapping are not implemented.")
                    .ResolverNotImplemented();
            }
        }
        
        private void OnRelation(EntityType entityType, IGrouping<string, EmbeddingRelationType> relationTypeGroup,
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
        
        protected void OnInterface(EntityType type, IInterfaceTypeDescriptor d = null)
        {
            if (d == null) return;
            d.Name(type.Name + "Entity");
            d.Field("id").Type<NonNullType<IdType>>();
            d.Field("createdAt").Type<NonNullType<DateTimeType>>();
            d.Field("updatedAt").Type<NonNullType<DateTimeType>>();
            d.Field("_relation").Type<RelationType>();
        }

        protected void OnRelation(EmbeddingRelationType relationType, IInterfaceTypeDescriptor interfaceTypeDescriptor = null)
        {
            var type = GetType(relationType);
            interfaceTypeDescriptor?.Field(relationType.GetFieldName()).Type(type.WrapOutputType(relationType));
        }
        
        private void OnRelation(EntityType entityType, IGrouping<string, EmbeddingRelationType> relationTypeGroup,
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
        
        private OutputUnionType GetOutputUnionType(EntityType entityType, IGrouping<string, EmbeddingRelationType> relationTypeGroup)
        {
            if (relationTypeGroup.Any(r => !r.IsEntityType))
                throw new ArgumentException($"Found attributes with same relation name: {entityType.Name}.{relationTypeGroup.Key}");
            var graphTypes = relationTypeGroup.Where(r => r.IsEntityType)
                .Select(r => _creator.GetOntologyType(r.EntityType)).ToList();
            if (graphTypes.Any(t => !(t is ObjectType)))
                throw new ArgumentException($"Can not create union with non-object type {entityType.Name}.{relationTypeGroup.Key}");
            return _creator.GetOutputUnionType(entityType, relationTypeGroup.Key, graphTypes.OfType<ObjectType>());
        }

        // ----- Generic ----- //

        private IOutputType GetType(EmbeddingRelationType relationType)
        {
            var type = relationType.IsAttributeType
                ? GetAttributeType(relationType)
                : relationType.IsEntityType
                    ? _creator.GetOntologyType(relationType.EntityType)
                    : throw new ArgumentException(nameof(relationType)); 
            return type;
        }

        private IOutputType GetAttributeType(EmbeddingRelationType relationType)
        {
            var type = relationType.AttributeType;
            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                return _creator.GetMultipleOutputType(type.ScalarTypeEnum);
            return _creator.GetScalarOutputType(type.ScalarTypeEnum);
        }

        public IOntologyType NewOntologyType(EntityType type)
        {
            if (type.IsAbstract)
            {
                Action<IInterfaceTypeDescriptor> configure = d =>
                {
                    OnInterface(type, d);
                    foreach (var attr in type.AllProperties.GroupBy(r => r.Name))
                        OnRelation(type, attr, d);
                };
//                configure(null);
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
//                configure(null); // Recursive loop if trying to create types explicitly
                return new OntologyObjectType(configure);
            }
        }

        public void AddFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            var objectType = _creator.GetOntologyType(type);
            var collectionType = new CollectionType(type.Name, objectType); // TODO: new NonNullType() won't work
            descriptor.Field($"entity{type.Name}")
                .Type(objectType)
                .Argument("id", d => d.Type<NonNullType<IdType>>())
                .Resolver(ctx => Resolvers.ResolveEntity(ctx, type));
            descriptor.Field($"entity{type.Name}List")
                .Type(collectionType)
                .Argument("pagination", d => d.Type<NonNullType<InputObjectType<PaginationInput>>>())
                .Argument("filter", d => d.Type<InputObjectType<FilterInput>>())
                .Resolver(ctx => Resolvers.ResolveEntityList(ctx, type));
        }

        public void AddFields(IObjectTypeDescriptor descriptor, EntityType type, Operation operation)
        {
            if (operation != Operation.Create)
                AddFields(descriptor, type);
            else
                throw new NotImplementedException();
        }
    }
}