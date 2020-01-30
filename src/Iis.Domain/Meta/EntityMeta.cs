using System;

namespace Iis.Domain.Meta
{
    public class EntityMeta : IMeta
    {
        public int? SortOrder { get; set; }
        public bool? ExposeOnApi { get; set; }
        public bool? HasFewEntities { get; set; }
        public EntityOperation[] AcceptsEmbeddedOperations { get; set; }
        public FormField FormField { get; set; }
        public ContainerMeta Container { get; set; }
    }

    public class ContainerMeta
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
    }
}
