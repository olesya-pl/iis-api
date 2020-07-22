using Iis.Interfaces.Meta;
using System;

namespace Iis.Domain.Meta
{
    public class EntityMeta: IEntityMeta
    {
        public int? SortOrder { get; set; }
        public bool? ExposeOnApi { get; set; }
        public bool? HasFewEntities { get; set; }
        public EntityOperation[] AcceptsEmbeddedOperations { get; set; }
        public FormField FormField { get; set; }
        public ContainerMeta Container { get; set; }
        IFormField IEntityMeta.FormField => FormField;
        IContainerMeta IEntityMeta.Container => Container;
    }

    public class ContainerMeta: IContainerMeta
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
    }
}
