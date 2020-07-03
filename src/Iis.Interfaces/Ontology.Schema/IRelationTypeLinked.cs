using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IRelationTypeLinked: IRelationType
    {
        INodeTypeLinked INodeTypeModel { get; }
        INodeTypeLinked SourceType { get; }
        INodeTypeLinked TargetType { get; }
        bool IsIdentical(IRelationTypeLinked relationType, bool includeTargetType);
    }
}
