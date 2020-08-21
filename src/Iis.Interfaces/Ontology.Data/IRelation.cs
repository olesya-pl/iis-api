using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IRelation: IRelationBase
    {
        INode Node { get; }
        INode SourceNode { get; }
        INode TargetNode { get; }

        Kind TargetKind { get; }
        bool IsLinkToSeparateObject { get; }
        string TypeName { get; }
    }
}
