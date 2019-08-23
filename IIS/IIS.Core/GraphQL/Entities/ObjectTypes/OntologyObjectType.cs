using System;
using HotChocolate.Types;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public class OntologyObjectType : ObjectType, IOntologyType
    {
        public OntologyObjectType(Action<IObjectTypeDescriptor> configure) : base(configure)
        {
        }

        public static string GetName(Type type)
        {
            return $"Entity{type.Name}";
        }
    }
}
