using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Meta
{
    public interface IContainerMeta: IMeta
    {
        Guid Id { get; }
        string Title { get; }
        string Type { get; }
    }
}
