namespace Iis.Interfaces.Elastic
{
    public interface IIisElasticField
    {
        string Name { get; }
        string Alias { get; }
        bool IsExcluded { get; }
        int Fuzziness { get; }
        decimal Boost { get; }
        bool IsAggregated { get; }
    }
}