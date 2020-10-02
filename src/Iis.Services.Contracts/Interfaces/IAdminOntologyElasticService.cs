using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IAdminOntologyElasticService
    {
        StringBuilder Logger { get; set; }

        Task CreateOntologyMappingsAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default);
        Task DeleteOntologyIndexesAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default);
        Task FillOntologyIndexesAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default);
        Task FillOntologyIndexesFromMemoryAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default);
        bool IsIndexesValid(IEnumerable<string> indexes);
        Task DeleteIndexesAsync(IEnumerable<string> indexes, CancellationToken ct = default);
        Task CreateReportMappingsAsync(CancellationToken ct = default);
        Task FillReportIndexAsync(CancellationToken ct = default);
    }
}