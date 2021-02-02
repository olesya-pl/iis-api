using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IContainerMeta
    {
        Guid Id { get; }
        string Title { get; }
        string Type { get; }
    }
}
