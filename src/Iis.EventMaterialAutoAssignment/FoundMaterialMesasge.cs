using System;
using System.Collections.Generic;

namespace Iis.EventMaterialAutoAssignment
{
    public class FoundMaterialMessage
    {
        public IReadOnlyCollection<Guid> MaterialIds { get; set; }
        public Guid ConfigId { get; set; }
    }
}
