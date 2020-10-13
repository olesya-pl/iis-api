namespace Iis.Interfaces.Meta
{
    public interface IAttributeRelationMeta : IMeta
    {
        bool? IsAggregated { get; }
        string Formula { get; }
    }
}
