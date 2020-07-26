using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IEntityMeta : IMeta
    {
        int? SortOrder { get; set; }
        bool? ExposeOnApi { get; set; }
        bool? HasFewEntities { get; set; }
        EntityOperation[] AcceptsEmbeddedOperations { get; set; }
        IFormField FormField { get; }
        IContainerMeta Container { get; }
    }
}
