using System;
using HotChocolate.Types;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public class OntologyInterfaceType : InterfaceType, IOntologyType
    {
        public OntologyInterfaceType(Action<IInterfaceTypeDescriptor> configure) : base(configure)
        {
        }

        public static string GetName(Type type)
        {
            return OntologyObjectType.GetName(type);
        }
    }
}
