using System;
using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.GraphQL.Mutations;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public abstract class MutatorTypeCreator
    {
        protected readonly GraphQlTypeCreator TypeCreator;
        public Operation Operation { get; }
        public MutatorTypeCreator(GraphQlTypeCreator typeCreator, Operation operation)
        {
            TypeCreator = typeCreator;
            Operation = operation;
        }

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
    }
    
     // ----- CREATE ----- //
    public class CreateMutatorTypeCreator : MutatorTypeCreator
    {
        public CreateMutatorTypeCreator(GraphQlTypeCreator typeCreator) : base(typeCreator, Operation.Create)
        {
        }

        protected override void OnRelation(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            IInputType type = null;

            if (relationType.IsEntityType && relationType.AcceptsOperation(EntityOperation.Create))
                type = TypeCreator.GetEntityRelationToInputType(Operation.Create, relationType.EntityType)
                    .WrapInputType(relationType);

            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple && relationType.IsAttributeType)
                type = TypeCreator.GetMultipleInputType(Operation.Create, relationType.AttributeType)
                    .WrapInputType(relationType);

            if (type == null)
                base.OnRelation(relationType, objectTypeDescriptor);
            else
                objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
        }
    }

    // ----- UPDATE ----- //
    public class UpdateMutatorTypeCreator : MutatorTypeCreator
    {
        public UpdateMutatorTypeCreator(GraphQlTypeCreator typeCreator) : base(typeCreator, Operation.Update)
        {
        }
        
        protected override void OnRelation(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
            {
                var type = TypeCreator.GetRelationPatchType(relationType);
                objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
            }
            else
                base.OnRelation(relationType, objectTypeDescriptor);
        }
    }

    // ----- DELETE ----- //
    public class DeleteMutatorTypeCreator : MutatorTypeCreator
    {
        public DeleteMutatorTypeCreator(GraphQlTypeCreator typeCreator) : base(typeCreator, Operation.Delete)
        {
        }
    }
}