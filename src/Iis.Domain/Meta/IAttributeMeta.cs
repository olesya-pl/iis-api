using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Meta
{
    public interface IAttributeMeta: IMeta
    {
        IValidation Validation { get; set; }
        SearchType? Kind { get; set; }
    }
}
