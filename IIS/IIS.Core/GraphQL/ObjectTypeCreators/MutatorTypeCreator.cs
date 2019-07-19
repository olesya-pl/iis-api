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
        protected readonly Dictionary<string, ObjectType> KnownTypes;
        public string Operation { get; }
        public MutatorTypeCreator(Dictionary<string, ObjectType> knownTypes, string operation)
        {
            KnownTypes = knownTypes;
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
                .ScalarType(relationType.AttributeType);
        }

        public ObjectType CreateResponse(string entityTypeName)
        {
            return new ObjectType(d =>
            {
                d.Name($"{Operation}{entityTypeName}Response");
                d.Field("type").Type<StringType>()
                    .ResolverNotImplemented();
                d.Field("details").Type(KnownTypes[entityTypeName])
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

        public abstract void AddFields(IObjectTypeDescriptor descriptor, Type type);
    }

    public class MutatorInputType : InputObjectType
    {
        public MutatorInputType(Action<IInputObjectTypeDescriptor> configure) : base(configure)
        {
        }
    }
}