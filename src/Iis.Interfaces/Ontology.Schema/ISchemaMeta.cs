using Iis.Interfaces.Meta;

namespace Iis.OntologySchema.DataTypes
{
    public interface ISchemaMeta: IRelationMetaBase, IEntityMeta, IEntityRelationMeta, IAttributeRelationMeta
    {
        bool? ExposeOnApi { get; set; }
        bool? HasFewEntities { get; set; }
        int? SortOrder { get; set; }
        string Title { get; set; }
        string Formula { get; set; }
        string Format { get; set; }
        EntityOperation[] AcceptsEntityOperations { get; set; }
        EntityOperation[] AcceptsEmbeddedOperations { get; set; }
        string Type { get; set; }
        string[] TargetTypes { get; set; }
        IFormField FormField { get; set; }
        IContainerMeta Container { get; set; }
        bool Multiple { get; set; }
        IValidation Validation { get; set; }
        IInversedRelationMeta Inversed { get; set; }
    }
}