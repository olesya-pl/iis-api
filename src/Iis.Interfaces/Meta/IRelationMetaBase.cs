using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IRelationMetaBase : IMeta
    {
        int? SortOrder { get; set; }
        string Title { get; set; }
        IFormField FormField { get; set; }
        IContainerMeta Container { get; set; }
        bool Multiple { get; set; }
        IValidation Validation { get; set; }
    }
}
