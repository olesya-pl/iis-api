using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Language;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;
using Humanizer;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class ReadQueryTypeCreator : ITypeFieldPopulator
    {
        private readonly IGraphQlTypeProvider _typeProvider;

        public ReadQueryTypeCreator(IGraphQlTypeProvider typeProvider)
        {
            _typeProvider = typeProvider;
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
                objectTypeDescriptor.Field(relationType.TargetType.Name)
                    .Type(_typeProvider.GetOutputAttributeType(relationType.AttributeType).WrapOutputType(relationType))
                    .Resolver(ctx => Resolvers.ResolveRelation(ctx, relationType));
            else if (relationType.IsEntityType)
                objectTypeDescriptor.Field(relationType.TargetType.Name)
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
                interfaceTypeDescriptor.Field(relationType.TargetType.Name)
                    .Type(_typeProvider.GetOutputAttributeType(relationType.AttributeType).WrapOutputType(relationType));
            else if (relationType.IsEntityType)
                interfaceTypeDescriptor.Field(relationType.TargetType.Name)
                    .Type(CreateOutputType(relationType.EntityType).WrapOutputType(relationType));
            else
                throw new ArgumentException(nameof(relationType));
        }

        // ----- Generic ----- //

        public IOutputType CreateOutputType(EntityType type)
        {
            if (_typeProvider.OntologyTypes.ContainsKey(type.Name))
                return _typeProvider.OntologyTypes[type.Name];
            IOutputType outputType;
            if (type.IsAbstract)
                outputType = new InterfaceType(d =>
                {
                    OnInterface(type, d);
                    foreach (var attr in type.AllProperties)
                        OnRelation(attr, d);
                });
            else
                outputType = new ObjectType(d =>
                {
                    OnObject(type, d);
                    foreach (var attr in type.AllProperties)
                        OnRelation(attr, d);
                });
            _typeProvider.OntologyTypes.Add(type.Name, outputType);
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
            descriptor.Field(type.Name.Pluralize())
                .Type(collectionType)
                .ResolverNotImplemented();
        }
    }
    
    // Explicit interface declaration, that would be implemented by each EntityType
    public class EntityInterface : InterfaceType
    {
        protected override void Configure(IInterfaceTypeDescriptor descriptor)
        {
            descriptor.Field("id").Type<NonNullType<IdType>>();
            descriptor.Field("createdAt").Type<NonNullType<DateTimeType>>();
            descriptor.Field("updatedAt").Type<NonNullType<DateTimeType>>();
            descriptor.Field("_relation").Type<NotImplementedType>();
        }
    }
}