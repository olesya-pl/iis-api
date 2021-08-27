using System.Collections.Generic;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IElasticState
    {
        IReadOnlyCollection<string> EventIndexes { get; }
        IReadOnlyCollection<string> MaterialIndexes { get; }
        IReadOnlyCollection<string> OntologyIndexes { get; }
        IReadOnlyCollection<string> WikiIndexes { get; }
        IReadOnlyCollection<string> ObjectIndexes { get; }
        string ReportIndex { get; }
        IReadOnlyCollection<string> SignIndexes { get; }
        IReadOnlyCollection<string> ChangeHistoryIndexes { get; }
        Dictionary<string, IEnumerable<string>> FieldsToExcludeByIndex { get; }
    }
}