using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities.InputTypes.Mutations
{
    public class MultipleInputType : InputObjectType
    {
        private readonly IInputType _inputType;
        private readonly Operation _operation;

        private readonly string _scalarName;

        public MultipleInputType(Operation operation, string scalarName, IInputType inputType)
        {
            _scalarName = scalarName;
            _inputType = inputType;
            _operation = operation;
        }

        public static string GetName(Operation operation, string scalarName)
        {
            return $"MultipleInput_{operation.Short()}_{scalarName}";
        }

        protected override void Configure(IInputObjectTypeDescriptor d)
        {
            d.Name(GetName(_operation, _scalarName));
            d.Field("value").Type(new NonNullType(_inputType));
            if (_operation == Operation.Update)
                d.Field("id").Type<NonNullType<IdType>>();
        }
    }
}
