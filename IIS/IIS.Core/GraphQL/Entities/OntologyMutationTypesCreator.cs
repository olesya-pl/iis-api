using System;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.InputTypes.Mutations;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.Entities
{
    public abstract class OntologyMutationTypesCreator
    {
        protected readonly TypeRepository TypeRepository;

        public OntologyMutationTypesCreator(TypeRepository typeRepository, Operation operation)
        {
            TypeRepository = typeRepository;
            Operation = operation;
        }

        public Operation Operation { get; }

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

        protected virtual void OnRelation(EmbeddingRelationType relationType,
            IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            var type = relationType.IsAttributeType
                ? TypeRepository.GetInputAttributeType(relationType.AttributeType).WrapInputType(relationType)
                : TypeRepository.GetType<EntityRelationInputType>().WrapInputType(relationType);
            objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
        }
    }

    // ----- CREATE ----- //
    public class CreateOntologyMutationTypesCreator : OntologyMutationTypesCreator
    {
        public CreateOntologyMutationTypesCreator(TypeRepository typeRepository) : base(typeRepository,
            Operation.Create)
        {
        }

        protected override void OnRelation(EmbeddingRelationType relationType,
            IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            IInputType type = null;

            if (relationType.IsComputed() || relationType.IsInversed) return;

            if (relationType.IsEntityType && relationType.AcceptsOperation(EntityOperation.Create))
                type = TypeRepository.GetEntityRelationToInputType(Operation.Create, relationType.EntityType)
                    .WrapInputType(relationType);

            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple && relationType.IsAttributeType)
                type = TypeRepository.GetMultipleInputType(Operation.Create, relationType.AttributeType)
                    .WrapInputType(relationType);

            if (type == null)
                base.OnRelation(relationType, objectTypeDescriptor);
            else
                objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
        }
    }

    // ----- UPDATE ----- //
    public class UpdateOntologyMutationTypesCreator : OntologyMutationTypesCreator
    {
        public UpdateOntologyMutationTypesCreator(TypeRepository typeRepository) : base(typeRepository,
            Operation.Update)
        {
        }

        protected override void OnRelation(EmbeddingRelationType relationType,
            IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
            {
                var type = TypeRepository.GetRelationPatchType(relationType);
                objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
            }
            else
            {
                var type = relationType.IsAttributeType
                    ? TypeRepository.GetInputAttributeType(relationType.AttributeType)
                    : TypeRepository.GetType<EntityRelationInputType>();
                objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
            }
        }
    }

    // ----- DELETE ----- //
    public class DeleteOntologyMutationTypesCreator : OntologyMutationTypesCreator
    {
        public DeleteOntologyMutationTypesCreator(TypeRepository typeRepository) : base(typeRepository,
            Operation.Delete)
        {
        }
    }
}
