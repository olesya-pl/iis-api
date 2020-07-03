using System;
using HotChocolate.Types;
using IIS.Core.Ontology;
using Iis.Domain;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public class OntologyInterfaceType : InterfaceType, IOntologyType
    {
        public OntologyInterfaceType(Action<IInterfaceTypeDescriptor> configure) : base(configure)
        {
        }

        public static string GetName(INodeTypeModel type)
        {
            return OntologyObjectType.GetName(type);
        }
    }
}
