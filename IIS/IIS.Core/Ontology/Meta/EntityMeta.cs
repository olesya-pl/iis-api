using Iis.Domain.Meta;

namespace IIS.Core.Ontology.Meta
{
    public class EntityMeta : IMeta
    {
        public int? SortOrder { get; set; }
        public bool? ExposeOnApi { get; set; }
        public bool? HasFewEntities { get; set; }
        public EntityOperation[] AcceptsEmbeddedOperations { get; set; }
    }
}
