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
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class ReadQueryTypeCreator : ITypeFieldPopulator
    {
        private readonly IGraphQlTypeRepository _typeRepository;

        public ReadQueryTypeCreator(IGraphQlTypeRepository typeRepository)
        {
            _typeRepository = typeRepository;
        }

        // ----- Object creation ----- //

        protected void OnObject(EntityType type, IObjectTypeDescriptor d)
        {
            d.Name(type.Name + "Entity");
            foreach (var parentInterface in type.AllParents.Select(CreateOutputType).OfType<InterfaceType>())
                d.Interface(parentInterface);
            d.Interface<EntityInterface>();
            d.Field("id").Type<NonNullType<IdType>>().Resolver(ctx => Resolvers.ResolveId(ctx));
            d.Field("createdAt").Type<NonNullType<DateTimeType>>().ResolverNotImplemented();
            d.Field("updatedAt").Type<NonNullType<DateTimeType>>().ResolverNotImplemented();
            d.FieldNotImplemented("_relation");
        }

        protected void OnRelation(EmbeddingRelationType relationType, IObjectTypeDescriptor objectTypeDescriptor)
        {
            if (relationType.IsAttributeType)
                objectTypeDescriptor.Field(relationType.GetFieldName())
                    .Type(_typeRepository.GetOutputAttributeType(relationType.AttributeType).WrapOutputType(relationType))
                    .Resolver(ctx => Resolvers.ResolveRelation(ctx, relationType));
            else if (relationType.IsEntityType)
                objectTypeDescriptor.Field(relationType.GetFieldName())
                    .Type(CreateOutputType(relationType.EntityType).WrapOutputType(relationType))
                    .Resolver(ctx => Resolvers.ResolveRelation(ctx, relationType));
            else
                throw new ArgumentException(nameof(relationType));
        }
        
        // ----- Interfaces creation ----- //
        
        protected void OnInterface(EntityType type, IInterfaceTypeDescriptor d)
        {
            d.Name(type.Name + "Entity");
            d.Field("id").Type<NonNullType<IdType>>();
            d.Field("createdAt").Type<NonNullType<DateTimeType>>();
            d.Field("updatedAt").Type<NonNullType<DateTimeType>>();
            d.Field("_relation").Type<NotImplementedType>();
        }

        protected void OnRelation(EmbeddingRelationType relationType, IInterfaceTypeDescriptor interfaceTypeDescriptor)
        {
            if (relationType.IsAttributeType)
                interfaceTypeDescriptor.Field(relationType.GetFieldName())
                    .Type(_typeRepository.GetOutputAttributeType(relationType.AttributeType).WrapOutputType(relationType));
            else if (relationType.IsEntityType)
                interfaceTypeDescriptor.Field(relationType.GetFieldName())
                    .Type(CreateOutputType(relationType.EntityType).WrapOutputType(relationType));
            else
                throw new ArgumentException(nameof(relationType));
        }

        // ----- Generic ----- //

        public IOutputType CreateOutputType(EntityType type)
        {
            if (_typeRepository.OntologyTypes.ContainsKey(type.Name))
                return _typeRepository.OntologyTypes[type.Name];
            IOutputType outputType;
            if (type.IsAbstract)
                outputType = new OntologyInterfaceType(d =>
                {
                    OnInterface(type, d);
                    foreach (var attr in type.AllProperties)
                        OnRelation(attr, d);
                });
            else
                outputType = new OntologyObjectType(d =>
                {
                    OnObject(type, d);
                    foreach (var attr in type.AllProperties)
                        OnRelation(attr, d);
                });
            _typeRepository.OntologyTypes.Add(type.Name, outputType);
            return outputType;
        }

        public void AddFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            var objectType = CreateOutputType(type);
            var collectionType = new CollectionType(type.Name, objectType); // TODO: new NonNullType() won't work
            descriptor.Field(type.Name)
                .Type(objectType)
                .Argument("id", d => d.Type<NonNullType<IdType>>())
                .ResolverNotImplemented();
            descriptor.Field(type.Name + "List")
                .Type(collectionType)
                .ResolverNotImplemented();
        }
    }
}