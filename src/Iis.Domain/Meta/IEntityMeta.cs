using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Meta
{
    public interface IEntityMeta: IMeta
    {
        int? SortOrder { get; set; }
        bool? ExposeOnApi { get; set; }
        bool? HasFewEntities { get; set; }
        EntityOperation[] AcceptsEmbeddedOperations { get; set; }
        FormField FormField { get; set; }
        ContainerMeta Container { get; set; }
    }
}
