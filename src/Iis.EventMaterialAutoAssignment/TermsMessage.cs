using System;
using System.Collections.Generic;

namespace Iis.EventMaterialAutoAssignment
{
    public class TermsMessage
    {
        public IReadOnlyCollection<string> Terms { get; set; }
        public IReadOnlyCollection<Guid> MaterialIds { get; set; }
        public Guid ConfigId { get; set; }
    }
}
