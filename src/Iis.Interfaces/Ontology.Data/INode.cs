using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

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
        
        INode GetSingleDirectProperty(string name);
        INode GetSingleProperty(IDotName dotName);
        INode GetSingleProperty(string dotName);
        bool AllValuesAreEmpty(IEnumerable<string> dotNames);
        bool HasTheSameValues(INode another, IEnumerable<string> dotNames);
        IDotNameValues GetComputedValues();
        string ResolveFormula(string formula);
    }
}
