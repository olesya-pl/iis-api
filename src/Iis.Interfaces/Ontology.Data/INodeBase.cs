using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface INodeBase
    {
        Guid Id { get; }
        Guid NodeTypeId { get; }
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
        bool IsArchived { get; }
    }
}
