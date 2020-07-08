using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Meta
{
    public interface IRelationMetaBase: IMeta
    {
        int? SortOrder { get; set; }
        string Title { get; set; }
        FormField FormField { get; set; }
        ContainerMeta Container { get; set; }
        bool Multiple { get; set; }
        Validation Validation { get; set; }
    }
}
