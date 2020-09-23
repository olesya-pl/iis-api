using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Data
{
    public interface INode: INodeBase
    {
        INodeTypeLinked NodeType { get; }

        IReadOnlyList<IRelation> IncomingRelations { get; }
        IReadOnlyList<IRelation> OutgoingRelations { get; }

        IAttribute Attribute { get; }
        IRelation Relation { get; }

        string Value { get; }
        public IDotNameValues GetDotNameValues();
        bool HasPropertyWithValue(string propertyName, string value);
    }
}
