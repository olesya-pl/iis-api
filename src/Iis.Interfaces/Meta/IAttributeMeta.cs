using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IAttributeMeta : IMeta
    {
        IValidation Validation { get; set; }
        SearchType? Kind { get; set; }
    }
}
