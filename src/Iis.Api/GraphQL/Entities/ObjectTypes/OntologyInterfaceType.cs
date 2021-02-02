using System;
using HotChocolate.Types;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.OntologySchema.DataTypes;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public class OntologyInterfaceType : InterfaceType, IOntologyType
    {
        public OntologyInterfaceType(Action<IInterfaceTypeDescriptor> configure) : base(configure)
        {
        }

        public static string GetName(INodeTypeLinked type)
        {
            return OntologyObjectType.GetName(type);
        }
    }
}
