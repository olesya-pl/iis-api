using System;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.Domain.Meta;

namespace IIS.Core.GraphQL.Entities.InputTypes.Mutations
{
    public class SingularRelationPatchType : InputObjectType
    {
        private readonly string _typeName;
        private readonly IInputType _createType;
        private readonly IInputType _updateType;
        private readonly IEmbeddingRelationTypeModel _relationType;

        public SingularRelationPatchType(IEmbeddingRelationTypeModel relationType, TypeRepository typeRepository)
        {
            _relationType = relationType;
            _typeName = GetName(relationType);
            if (relationType.IsAttributeType)
            {
                _createType = typeRepository.GetMultipleInputType(Operation.Create, relationType.IAttributeTypeModel);
                _updateType = typeRepository.GetMultipleInputType(Operation.Update, relationType.IAttributeTypeModel);
            }
            else if (relationType.IsEntityType)
            {
                _createType = relationType.AcceptsOperation(EntityOperation.Create)
                    ? typeRepository.GetEntityRelationToInputType(Operation.Create, relationType.EntityType)
                    : null;
                _updateType = relationType.AcceptsOperation(EntityOperation.Update)
                    ? new SingularRelationPatchUpdateWrapperType(_typeName,
                        typeRepository.GetEntityUnionInputType(Operation.Update, relationType.EntityType))
                    : null;
            }
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"SingleInput_{_typeName}");
            if (_createType == null)
                d.Field("create").Type<EntityRelationInputType>();
            else
                d.Field("create").Type(_createType);
            if (_updateType != null)
                d.Field("update").Type(_updateType);
        }

        public static string GetName(IEmbeddingRelationTypeModel relationType)
        {
            if (relationType.IsAttributeType)
                return relationType.IAttributeTypeModel.ScalarTypeEnum.ToString();
            if (relationType.IsEntityType)
            {
                var ops = relationType.GetOperations();
                if (ops == null || ops.Length == 0)
                    return "EntityRelationInput";
                return $"{OntologyObjectType.GetName(relationType.EntityType)}_{RelationPatchType.GetAbbreviation(ops)}";
            }

            throw new ArgumentException(nameof(relationType));
        }
    }

    public class SingularRelationPatchUpdateWrapperType : InputObjectType
    {
        private readonly EntityUnionInputType _inputType;
        private readonly string _typeName;

        public SingularRelationPatchUpdateWrapperType(string typeName, EntityUnionInputType inputType)
        {
            _typeName = typeName;
            _inputType = inputType;
        }

        protected override void Configure(IInputObjectTypeDescriptor descriptor)
        {
            descriptor.Name(_typeName + "_UpdateInput");
            descriptor.Field("target").Type(new NonNullType(_inputType));
        }
    }
}
