using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface INodeTypeLinked: INodeType
    {
        IReadOnlyList<IRelationTypeLinked> IncomingRelations { get; }
        IReadOnlyList<IRelationTypeLinked> OutgoingRelations { get; }
        IAttributeType AttributeType { get; set; }
        IRelationTypeLinked RelationType { get; set; }
    }
}
