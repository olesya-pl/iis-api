using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class UpdateMutatorTypeCreator : MutatorTypeCreator, ITypeFieldPopulator
    {
        public UpdateMutatorTypeCreator(GraphQlTypeCreator typeCreator) : base(typeCreator, "Update")
        {
        }
        
        protected override void OnRelation(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            IInputType type = null;

            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
            {
                if (relationType.IsEntityType && /* relationType.AcceptsEntityOperations == Update */ true)
                    type = TypeCreator.GetEntityRelationPatchType(relationType.EntityType)
                        .WrapInputType(relationType);
                
                if (relationType.IsAttributeType)
                    type = TypeCreator.GetAttributeRelationPatchType(relationType.AttributeType)
                        .WrapInputType(relationType);
            }
            
            if (type == null)
                base.OnRelation(relationType, objectTypeDescriptor);
            else
                objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
        }

        public override void AddFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            descriptor.Field(Operation + type.Name)
                .Type(TypeCreator.GetMutatorResponseType(this, type))
                .Argument("id", d => d.Type<NonNullType<IdType>>())
//                .Argument("data", d => d.Type(new NonNullType(CreateObjectType(type))))
                .Argument("data", d =>
                    d.Type(TypeCreator.GetMutatorInputType(this, type)))
                .ResolverNotImplemented();
        }
        
        // ----- UpdateMultipleInputType ----- //

        public UpdateMultipleInputType NewUpdateMultipleInputType(AttributeType attributeType)
        {
            var inputType = TypeCreator.GetInputAttributeType(attributeType);
            var name = attributeType.ScalarTypeEnum.ToString();
            return new UpdateMultipleInputType(name, inputType);
        }
        
        // ----- UpdateEntityRelationToTargetInputType ----- //

        public UpdateEntityRelationToInputType NewUpdateEntityRelationToInputType(EntityType type)
        {
            return new UpdateEntityRelationToInputType(type,
                TypeCreator.GetUpdateEntityRelationToTargetInputType(type));
        }

        public UpdateEntityRelationToTargetInputType NewUpdateEntityRelationToTargetInputType(EntityType type)
        {
            return new UpdateEntityRelationToTargetInputType(type, TypeCreator, this);
        }
        
        // ----- Patches ----- //

        public EntityRelationPatchType NewEntityRelationPatchType(EntityType type)
        {
            return new EntityRelationPatchType(type, TypeCreator);
        }

        public AttributeRelationPatchType NewAttributeRelationPatchType(AttributeType type)
        {
            return new AttributeRelationPatchType(type, TypeCreator);
        }
    }
}