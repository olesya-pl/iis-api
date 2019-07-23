using System;
using System.Collections.Generic;
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

        protected void OnObject(Type type, IObjectTypeDescriptor d)
        {
            d.Name(type.Name + "Entity");
            d.Interface<EntityInterface>();
            d.Field("id").Type<NonNullType<IdType>>().Resolver(ctx => Resolvers.ResolveId(ctx));
        }

        protected void OnEntityRelation(EmbeddingRelationType relationType, IObjectTypeDescriptor objectTypeDescriptor)
        {
            var type = CreateObjectType(relationType.EntityType).WrapOutputType(relationType);
            objectTypeDescriptor.Field(relationType.TargetType.Name)
                .Type(new NonNullType(type))
                .Resolver(ctx => Resolvers.ResolveRelation(ctx, relationType));
        }

        protected void OnAttributeRelation(EmbeddingRelationType relationType, IObjectTypeDescriptor objectTypeDescriptor)
        {
            objectTypeDescriptor.Field(relationType.TargetType.Name)
                .ScalarType(relationType, _typeProvider)
                .Resolver(ctx => Resolvers.ResolveRelation(ctx, relationType));
        }

        public ObjectType CreateObjectType(Type type)
        {
            if (_typeProvider.OntologyTypes.ContainsKey(type.Name))
                return _typeProvider.OntologyTypes[type.Name];
            var ot = new ObjectType(d =>
            {
                OnObject(type, d);
                foreach (var attr in type.AllProperties)
                    CreateField(attr, d);
            });
            _typeProvider.OntologyTypes.Add(type.Name, ot);
            return ot;
        }
        
        protected void CreateField(EmbeddingRelationType relationType, IObjectTypeDescriptor objectTypeDescriptor)
        {
            if (relationType.IsAttributeType)
                OnAttributeRelation(relationType, objectTypeDescriptor);
            else if (relationType.IsEntityType)
                OnEntityRelation(relationType, objectTypeDescriptor);
            else
                throw new ArgumentException(nameof(relationType));
        }

        public void AddFields(IObjectTypeDescriptor descriptor, Type type)
        {
            var objectType = CreateObjectType(type);
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
        }
    }
}