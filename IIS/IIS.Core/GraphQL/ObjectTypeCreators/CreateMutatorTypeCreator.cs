using System;
using System.Collections.Generic;
using HotChocolate.Types;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.ObjectTypeCreators
{
    public class CreateMutatorTypeCreator : MutatorTypeCreator
    {
        public CreateMutatorTypeCreator(GraphQlTypeCreator typeCreator) : base(typeCreator, "Create")
        {
        }

        // ----- MutatorInputType ----- //
        
        protected override void OnRelation(EmbeddingRelationType relationType, IInputObjectTypeDescriptor objectTypeDescriptor = null)
        {
            IInputType type = null;

            if (relationType.IsEntityType && /* relationType.AcceptsEntityOperations == Create */ true)
                type = TypeCreator.GetCreateEntityRelationToInputType(relationType.EntityType)
                    .WrapInputType(relationType);

            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple && relationType.IsAttributeType)
                type = TypeCreator.GetCreateMultipleInputType(relationType.AttributeType)
                    .WrapInputType(relationType);

            if (type == null)
                base.OnRelation(relationType, objectTypeDescriptor);
            else
                objectTypeDescriptor?.Field(relationType.GetFieldName()).Type(type);
        }
        
        // ----- CreateMultipleInputType ----- //

        public CreateMultipleInputType NewCreateMultipleInputType(AttributeType attributeType)
        {
            var inputType = TypeCreator.GetInputAttributeType(attributeType);
            var name = attributeType.ScalarTypeEnum.ToString();
            return new CreateMultipleInputType(name, inputType);
        }
        
        // ----- CreateEntityRelationToTargetInputType ----- //

        public CreateEntityRelationToInputType NewCreateEntityRelationToInputType(EntityType type)
        {
            return new CreateEntityRelationToInputType(type,
                TypeCreator.GetCreateEntityRelationToTargetInputType(type));
        }

        public CreateEntityRelationToTargetInputType NewCreateEntityRelationToTargetInputType(EntityType type)
        {
            return new CreateEntityRelationToTargetInputType(type, TypeCreator, this);
        }
        
        // ----- END ----- //

        public override void AddFields(IObjectTypeDescriptor descriptor, EntityType type)
        {
            descriptor.Field(Operation + type.Name)
                .Type(TypeCreator.GetMutatorResponseType(this, type))
//                .Argument("data", d => d.Type(new NonNullType(CreateObjectType(type)))) // fail
//                .Argument("data", d => d.Type<NonNullType<StringType>>()) // good
//                .Argument("data", d => d.Type(TypeProvider.Scalars[Core.Ontology.ScalarType.String])) // ok
//                .Argument("data", d => d.Type(new NonNullType(TypeProvider.Scalars[Core.Ontology.ScalarType.String]))) // fail
                .Argument("data", d =>
                    d.Type(TypeCreator.GetMutatorInputType(this, type)))
                .ResolverNotImplemented();
        }

//        public override void CreateTypes(EntityType type)
//        {
//            base.CreateTypes(type);
//            foreach (var relationType in type.AllProperties)
//            {
//                if (relationType.IsAttributeType)
//                {
//                    var attr = TypeCreator.GetInputAttributeType(relationType.AttributeType);
//                    var name = relationType.AttributeType.ScalarTypeEnum.ToString();
//                    TypeCreator.GetOrCreate(name, () => new CreateMultipleInputType(name, attr));
//                }
//                else if (relationType.IsEntityType)
//                {
//                    
//                }
//            }
//        }
    }
}