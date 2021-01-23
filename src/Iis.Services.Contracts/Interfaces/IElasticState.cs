using System.Collections.Generic;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IElasticState
    {
        List<string> EventIndexes { get; }
        List<string> MaterialIndexes { get; }
        List<string> OntologyIndexes { get; }
        List<string> WikiIndexes { get; }
        string ReportIndex { get; }
        List<string> SignIndexes { get; }
        Dictionary<string, IEnumerable<string>> FieldsToExcludeByIndex { get; }
    }
}