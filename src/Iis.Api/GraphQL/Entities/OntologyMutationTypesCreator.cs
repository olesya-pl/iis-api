using System;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.InputTypes.Mutations;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Meta;

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
        public MutatorInputType NewMutatorInputType(INodeTypeModel type)
        {
            var configure = new Action<IInputObjectTypeDescriptor>(d =>
            {
                d?.Name(MutatorInputType.GetName(Operation, type.Name));
                foreach (var attr in type.AllProperties.Where(p => !(p.IsInversed || p.IsComputed())))
                    OnRelation(attr, d);
            });
            configure(null); // Ensure creation of types before descriptor is called
            return new MutatorInputType(configure);
        }

        protected virtual void OnRelation(IEmbeddingRelationTypeModel relationType,
            IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            var type = relationType.IsAttributeType
                ? TypeRepository.GetInputAttributeType(relationType.AttributeType).WrapInputType(relationType)
                : TypeRepository.GetType<EntityRelationInputType>().WrapInputType(relationType);
            objectTypeDescriptor?.Field(relationType.Name).Type(type);
        }
    }

    // ----- CREATE ----- //
    public class CreateOntologyMutationTypesCreator : OntologyMutationTypesCreator
    {
        public CreateOntologyMutationTypesCreator(TypeRepository typeRepository) : base(typeRepository,
            Operation.Create)
        {
        }

        protected override void OnRelation(IEmbeddingRelationTypeModel relationType,
            IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            IInputType type = null;

            if (relationType.IsEntityType && relationType.AcceptsOperation(EntityOperation.Create))
                type = TypeRepository.GetEntityRelationToInputType(Operation.Create, relationType.EntityType)
                    .WrapInputType(relationType);

            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple && relationType.IsAttributeType)
                type = TypeRepository.GetMultipleInputType(Operation.Create, relationType.AttributeType)
                    .WrapInputType(relationType);

            if (type == null)
                base.OnRelation(relationType, objectTypeDescriptor);
            else
                objectTypeDescriptor?.Field(relationType.Name).Type(type);
        }
    }

    // ----- UPDATE ----- //
    public class UpdateOntologyMutationTypesCreator : OntologyMutationTypesCreator
    {
        public UpdateOntologyMutationTypesCreator(TypeRepository typeRepository) : base(typeRepository,
            Operation.Update)
        {
        }

        protected override void OnRelation(IEmbeddingRelationTypeModel relationType,
            IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            IInputType type;
            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                type = TypeRepository.GetRelationPatchType(relationType);
            else if (relationType.IsAttributeType)
                type = TypeRepository.GetInputAttributeType(relationType.AttributeType);
            else if (relationType.AcceptsOperation(EntityOperation.Update))
                type = TypeRepository.GetSingularRelationPatchType(relationType);
            else
                type = TypeRepository.GetType<EntityRelationInputType>();
//            else if (relationType.IsEntityType && relationType.AcceptsOperation(EntityOperation.Update))
//            {
//                type = TypeRepository.GetEntityRelationToInputType(Operation.Update, relationType.EntityType)
//                    .WrapInputType(relationType);
//            }
//            else
//            {
//                type = relationType.IsAttributeType
//                    ? TypeRepository.GetInputAttributeType(relationType.IAttributeTypeModel)
//                    : TypeRepository.GetType<EntityRelationInputType>();
//            }
            objectTypeDescriptor?.Field(relationType.Name).Type(type);
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
