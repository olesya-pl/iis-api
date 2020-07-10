using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IContainerMeta : IMeta
    {
        Guid Id { get; }
        string Title { get; }
        string Type { get; }
    }
}
