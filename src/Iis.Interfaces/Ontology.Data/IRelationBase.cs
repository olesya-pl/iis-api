using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IRelationBase
    {
        Guid Id { get; }
        Guid SourceNodeId { get; }
        Guid TargetNodeId { get; }
    }
}
