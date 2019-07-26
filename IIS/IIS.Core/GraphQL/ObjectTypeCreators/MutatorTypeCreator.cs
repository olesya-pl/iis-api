using System;
using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.GraphQL.Mutations;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public abstract class MutatorTypeCreator
    {
        protected readonly GraphQlTypeCreator TypeCreator;
        public string Operation { get; }
        public MutatorTypeCreator(GraphQlTypeCreator typeCreator, string operation)
        {
            TypeCreator = typeCreator;
            Operation = operation;
        }

        // ----- MutatorInputType ----- //
        
        // this return value should not be wrapped in NonNullType()
        public MutatorInputType NewMutatorInputType(Type type)
        {
            var configure = new Action<IInputObjectTypeDescriptor>(d =>
            {
                d?.Name(MutatorInputType.GetName(Operation, type.Name));
                foreach (var attr in type.AllProperties)
                    OnRelation(attr, d);
            });
            configure(null); // Ensure creation of types before descriptor is called
            return new MutatorInputType(configure);
        }

        protected virtual void OnRelation(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            var type = relationType.IsAttributeType
                ? TypeCreator.GetInputAttributeType(relationType.AttributeType).WrapInputType(relationType)
                : TypeCreator.GetType<EntityRelationInputType>().WrapInputType(relationType);
            objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
        }
        
        // ----- MutatorResponseType ----- //

        public MutatorResponseType NewMutatorResponseType(Type type)
        {
            return new MutatorResponseType(Operation, type.Name, TypeCreator.GetOntologyType(type));
        }

        // ----- Types end ----- //

        public abstract void AddFields(IObjectTypeDescriptor descriptor, EntityType type);

//        public virtual void CreateTypes(EntityType type)
//        {
//            NewMutatorInputType(type);
//            NewMutatorResponseType(type);
//        }
    }
}