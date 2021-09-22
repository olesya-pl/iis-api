using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IInversedMeta
    {
        string Code { get; }
        string Title { get; }
        bool Multiple { get; }
    }
}
