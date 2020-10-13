namespace Iis.Interfaces.Elastic
{
    public interface IIisElasticField
    {
        string Name { get; }
        bool IsExcluded { get; }
        int Fuzziness { get; }
        decimal Boost { get; }
        bool IsAggregated { get; }
    }
}