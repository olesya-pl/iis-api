using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IRelationType
    {
        Guid Id { get; }
        Guid SourceTypeId { get; }
        Guid TargetTypeId { get; }
    }
}
