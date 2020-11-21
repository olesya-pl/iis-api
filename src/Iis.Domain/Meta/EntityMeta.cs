using Iis.Interfaces.Meta;
using System;

namespace Iis.Domain.Meta
{
    public class EntityMeta: MetaBase
    {
    }

    public class ContainerMeta: IContainerMeta
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
    }
}
