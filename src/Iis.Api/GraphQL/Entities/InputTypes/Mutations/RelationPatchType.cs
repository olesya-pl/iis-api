using System;
using System.Linq;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;

namespace IIS.Core.GraphQL.Entities.InputTypes.Mutations
{
    // Used for update of multiple relations
    public class RelationPatchType : InputObjectType
    {
        private readonly string _typeName;
        private readonly IInputType _createType;
        private readonly IInputType _updateType;
        private readonly IEmbeddingRelationTypeModel _relationType;

        public RelationPatchType(IEmbeddingRelationTypeModel relationType, TypeRepository typeRepository)
        {
            _relationType = relationType;
            _typeName = GetName(relationType);
            if (relationType.IsAttributeType)
            {
                _createType = typeRepository.GetMultipleInputType(Operation.Create, relationType.AttributeType);
                _updateType = typeRepository.GetMultipleInputType(Operation.Update, relationType.AttributeType);
            }
            else if (relationType.IsEntityType)
            {
                _createType = relationType.AcceptsOperation(EntityOperation.Create)
                    ? typeRepository.GetEntityRelationToInputType(Operation.Create, relationType.EntityType)
                    : null;
                _updateType = relationType.AcceptsOperation(EntityOperation.Update)
                    ? typeRepository.GetEntityRelationToInputType(Operation.Update, relationType.EntityType)
                    : null;
            }
        }

        public static string GetName(IEmbeddingRelationTypeModel relationType)
        {
            if (relationType.IsAttributeType)
                return relationType.AttributeType.ScalarTypeEnum.ToString();
            if (relationType.IsEntityType)
            {
                var ops = relationType.GetOperations();
                if (ops == null || ops.Length == 0)
                    return "EntityRelationInput";
                return $"{OntologyObjectType.GetName(relationType.EntityType)}_{GetAbbreviation(ops)}";
            }

            throw new ArgumentException(nameof(relationType));
        }

        public static string GetAbbreviation(EntityOperation[] ops)
        {
            return ops == null ? null : new string(ops.Select(o => o.ToString()[0]).ToArray());
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name($"PatchInput_{_typeName}");
            SetDescription(d);
            d.Field("delete").Type<ListType<NonNullType<IdType>>>();
            if (_createType == null)
                d.Field("create").Type<ListType<NonNullType<EntityRelationInputType>>>();
            else
                d.Field("create").Type(new ListType(new NonNullType(_createType)));
            if (_updateType == null)
                d.Field("update").Type<ListType<NonNullType<UpdateEntityRelationInputType>>>();
            else
                d.Field("update").Type(new ListType(new NonNullType(_updateType)));
        }

        protected void SetDescription(IInputObjectTypeDescriptor d)
        {
            var meta = _relationType.Meta;
            if (_relationType.IsAttributeType)
            {
                d.Description($"Patch array of {_typeName} attribute.");
            }
            else
            {
                var ops = _relationType.GetOperations();
                var description = $"Patch array of {_relationType.EntityType.Name} entity type.";
                if (ops != null)
                    description += $" Accepts entity operations: {string.Join(", ", ops)}";
                d.Description(description);
            }
        }
    }
}
