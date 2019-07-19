using System;
using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class EntityTypesCreator
    {
        protected readonly Dictionary<string, ObjectType> KnownTypes;
        
        public EntityTypesCreator(Dictionary<string, ObjectType> knownTypes)
        {
            KnownTypes = knownTypes;
        }

        protected void OnObject(Type type, IObjectTypeDescriptor d)
        {
            d.Name(type.Name + "Entity");
            d.Interface<EntityInterface>();
            d.Field("id").Type<IdType>().Resolver(ctx => Resolvers.ResolveId(ctx));
        }

        protected void OnEntityRelation(EmbeddingRelationType relationType, IObjectTypeDescriptor objectTypeDescriptor)
        {
            var type = CreateObjectType(relationType.EntityType);
            objectTypeDescriptor.Field(relationType.TargetType.Name)
                .Type(type)
                .Resolver(ctx => Resolvers.ResolveRelation(ctx, relationType));
        }

        protected void OnAttributeRelation(EmbeddingRelationType relationType, IObjectTypeDescriptor objectTypeDescriptor)
        {
            objectTypeDescriptor.Field(relationType.TargetType.Name)
                .ScalarType(relationType.AttributeType)
                .Resolver(ctx => Resolvers.ResolveRelation(ctx, relationType));
        }

        public ObjectType CreateObjectType(Type type)
        {
            if (KnownTypes.ContainsKey(type.Name))
                return KnownTypes[type.Name];
            var ot = new ObjectType(d =>
            {
                OnObject(type, d);
                foreach (var attr in type.AllProperties)
                    CreateField(attr, d);
            });
            KnownTypes.Add(type.Name, ot);
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
    }
}