using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IRelationMetaBase : IMeta
    {
        int? SortOrder { get; set; }
        string Title { get; set; }
        IFormField FormField { get; }
        IContainerMeta Container { get; }
        bool Multiple { get; set; }
        IValidation Validation { get; }
    }
}
