﻿using System.Collections.Generic;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IElasticState
    {
        List<string> EventIndexes { get; }
        Dictionary<string, string> HistoricalOntologyIndexes { get; }
        List<string> MaterialIndexes { get; }
        List<string> OntologyIndexes { get; }
        string ReportIndex { get; }
        List<string> SignIndexes { get; }
        Dictionary<string, IEnumerable<string>> FieldsToExcludeByIndex { get; }
    }
}