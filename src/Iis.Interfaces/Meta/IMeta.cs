namespace Iis.Interfaces.Meta
{
    public enum EntityOperation : byte
    {
        Create, Update, Delete
    }
    public interface IMeta
    {
        int? SortOrder { get; }
        bool? ExposeOnApi { get; }
        bool? HasFewEntities { get; }
        EntityOperation[] AcceptsEmbeddedOperations { get; }
        IFormField FormField { get; }
        IContainerMeta Container { get; }
        bool? IsAggregated { get; }
        string Formula { get; }
        string Title { get; }
        bool Multiple { get; }
        IValidation Validation { get; }
        EntityOperation[] AcceptsEntityOperations { get; set; }
        string Type { get; set; }
        IMeta Inversed { get; }
        string[] TargetTypes { get; set; }
        string Code { get; set; }
        bool Editable { get; set; }
    }
}