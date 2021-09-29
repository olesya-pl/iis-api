using System;
using HotChocolate.Types;
using IIS.Core.Ontology;
using Iis.Domain;
using Iis.OntologySchema.DataTypes;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public class OntologyObjectType : ObjectType, IOntologyType
    {
        public OntologyObjectType(Action<IObjectTypeDescriptor> configure) : base(configure)
        {
        }

        public static string GetName(INodeTypeLinked type)
        {
            return $"Entity{type.Name}";
        }
    }
}
