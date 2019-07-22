using System;
using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.GraphQL.Mutations;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public abstract class MutatorTypeCreator
    {
        protected readonly IGraphQlTypeProvider TypeProvider;
        public string Operation { get; }
        public MutatorTypeCreator(IGraphQlTypeProvider typeProvider, string operation)
        {
            TypeProvider = typeProvider;
            Operation = operation;
        }

        protected void OnObject(Type type, IInputObjectTypeDescriptor d)
        {
            d.Name($"{Operation}{type.Name}Input");
        }

        protected void OnEntityRelation(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor)
        {
            objectTypeDescriptor.Field(relationType.TargetType.Name)
                .Type<InputObjectType<EntityRelationInput>>();
        }

        protected void OnAttributeRelation(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor)
        {
            objectTypeDescriptor.Field(relationType.TargetType.Name)
                .Type(TypeProvider.GetInputAttributeType(relationType.AttributeType));
        }

        public ObjectType CreateResponse(string entityTypeName)
        {
            return new ObjectType(d =>
            {
                d.Name($"{Operation}{entityTypeName}Response");
                d.Field("type").Type<StringType>()
                    .ResolverNotImplemented();
                d.Field("details").Type(TypeProvider.OntologyTypes[entityTypeName])
                    .ResolverNotImplemented();
            });
        }

        public InputObjectType CreateObjectType(Type type)
        {
            return new MutatorInputType(d =>
            {
                OnObject(type, d);
                foreach (var attr in type.AllProperties)
                    CreateField(attr, d);
            });
        }
        
        protected void CreateField(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor)
        {
            if (relationType.IsAttributeType)
                OnAttributeRelation(relationType, objectTypeDescriptor);
            else if (relationType.IsEntityType)
                OnEntityRelation(relationType, objectTypeDescriptor);
            else
                throw new ArgumentException(nameof(relationType));
        }

        public abstract void AddFields(IObjectTypeDescriptor descriptor, EntityType type);
    }

    public class MutatorInputType : InputObjectType
    {
        public MutatorInputType(Action<IInputObjectTypeDescriptor> configure) : base(configure)
        {
        }
    }
}