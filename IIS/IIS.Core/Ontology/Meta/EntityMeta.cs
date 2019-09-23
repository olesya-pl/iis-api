namespace IIS.Core.Ontology.Meta
{
    public class EntityMeta : IMeta
    {
        public bool? ExposeOnApi { get; set; }
        public bool? HasFewEntities { get; set; }
        public EntityOperation[] AcceptsEmbeddedOperations { get; set; }
    }
}
