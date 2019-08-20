using System;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.Entities.InputTypes.Mutations
{
    public class EntityRelationToInputType : InputObjectType
    {
        private readonly Operation _operation;
        private readonly EntityUnionInputType _target;
        private readonly Type _type;

        public EntityRelationToInputType(Operation operation, Type type, EntityUnionInputType target)
        {
            _operation = operation;
            _target = target;
            _type = type;
        }

        public static string GetName(Operation operation, Type type)
        {
            return $"RelationTo_{operation.Short()}_{OntologyObjectType.GetName(type)}";
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name(GetName(_operation, _type));
//            d.Field("startsAt").Type<DateTimeType>();
//            d.Field("endsAt").Type<DateTimeType>();
            if (_operation == Operation.Create)
            {
                d.Description("Specify relation to existing object or create new with target field.");
                d.Field("targetId").Type<IdType>();
                d.Field("target").Type(_target);
            }
            else if (_operation == Operation.Update)
            {
                d.Description("Specify object id that you wish to update");
                d.Field("id").Type<NonNullType<IdType>>();
                d.Field("target").Type(new NonNullType(_target));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
