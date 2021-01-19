using Iis.Interfaces.Enums;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IAdminOntologyElasticService
    {
        StringBuilder Logger { get; set; }

        Task CreateIndexWithMappingsAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default);
        Task DeleteIndexesAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default);
        Task FillIndexesFromMemoryAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default);
        Task FillIndexesFromMemoryAsync(IEnumerable<string> indexes, IEnumerable<string> fieldsToExclude, CancellationToken ct = default);
        Task DeleteIndexesAsync(IEnumerable<string> indexes, CancellationToken ct = default);
        Task CreateReportIndexWithMappingsAsync(CancellationToken ct = default);
        Task FillReportIndexAsync(CancellationToken ct = default);
        Task AddAliasesToIndexAsync(AliasType type, CancellationToken ct = default);
    }
}