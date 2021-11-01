namespace Iis.Interfaces.Elastic
{
    public interface IGroupedAggregationNameGenerator
    {
        string GetUniqueAggregationName();
    }
}