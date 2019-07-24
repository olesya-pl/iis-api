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

        protected void OnRelation(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor)
        {
            
            if (relationType.IsAttributeType)
                objectTypeDescriptor.Field(relationType.TargetType.Name)
                    .Type(TypeProvider.GetInputAttributeType(relationType.AttributeType).WrapInputType(relationType));
            else if (relationType.IsEntityType)
                objectTypeDescriptor.Field(relationType.TargetType.Name)
                    .Type(TypeProvider.GetType<InputObjectType<EntityRelationInput>>().WrapInputType(relationType));
            else
                throw new ArgumentException(nameof(relationType));
        }

        public ObjectType CreateResponse(string entityTypeName)
        {
            return new ObjectType(d =>
            {
                d.Name($"{Operation}{entityTypeName}Response");
                d.Field("type").Type<NonNullType<StringType>>()
                    .ResolverNotImplemented();
                d.Field("details").Type(new NonNullType(TypeProvider.OntologyTypes[entityTypeName]))
                    .ResolverNotImplemented();
            });
        }

        // this return value should not be wrapped in NonNullType()
        public InputObjectType CreateObjectType(Type type)
        {
            return new MutatorInputType(d =>
            {
                OnObject(type, d);
                foreach (var attr in type.AllProperties)
                    OnRelation(attr, d);
            });
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