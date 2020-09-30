using System.Collections.Generic;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IElasticState
    {
        bool UseElastic { get; }
        List<string> EventIndexes { get; }
        List<string> FeatureIndexes { get; }
        Dictionary<string, string> HistoricalOntologyIndexes { get; }
        List<string> MaterialIndexes { get; }
        List<string> OntologyIndexes { get; }
        string ReportIndex { get; }
    }
}