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
        IDotNameValues GetDotNameValues();
        INode GetChildNode(string childTypeName);
        IReadOnlyList<INode> GetChildNodes(string childTypeName);
        bool HasPropertyWithValue(string propertyName, string value);
        INode GetSingleProperty(IDotName dotName);
        INode GetSingleProperty(string dotName);
        INode GetSingleDirectProperty(string name);
        bool AllValuesAreEmpty(IEnumerable<string> dotNames);
        bool HasTheSameValues(INode another, IEnumerable<string> dotNames);
        IDotNameValues GetComputedValues();
        string ResolveFormula(string formula);
    }
}
