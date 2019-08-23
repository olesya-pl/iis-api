using System;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities.InputTypes.Mutations
{
    public class MutatorInputType : InputObjectType
    {
        public MutatorInputType(Action<IInputObjectTypeDescriptor> configure) : base(configure)
        {
        }

        public static string GetName(Operation operation, string entityTypeName)
        {
            return $"{operation}{entityTypeName}Input";
        }
    }
}
