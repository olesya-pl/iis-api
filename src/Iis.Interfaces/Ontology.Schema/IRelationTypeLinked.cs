using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IRelationTypeLinked: IRelationType
    {
        INodeTypeLinked NodeType { get; }
        INodeTypeLinked SourceType { get; }
        INodeTypeLinked TargetType { get; }
        IRelationTypeLinked DirectRelationType { get; }
        IRelationTypeLinked InversedRelationType { get; }
        bool IsIdentical(IRelationTypeLinked relationType, bool includeTargetType);
    }
}
