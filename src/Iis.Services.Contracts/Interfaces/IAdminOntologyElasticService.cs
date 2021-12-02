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

        Task CreateIndexWithMappingsAsync(IReadOnlyCollection<string> indexes, CancellationToken cancellationToken = default);
        Task FillIndexesFromMemoryAsync(IEnumerable<string> indexes, CancellationToken cancellationToken = default);
        Task FillIndexesFromMemoryAsync(IEnumerable<string> indexes, IEnumerable<string> fieldsToExclude, CancellationToken cancellationToken = default);
        Task DeleteIndexesAsync(IEnumerable<string> indexes, CancellationToken cancellationToken = default);
        Task DeleteHistoricalIndexesAsync(IEnumerable<string> indexes, CancellationToken cancellationToken = default);
        Task CreateReportIndexWithMappingsAsync(CancellationToken cancellationToken = default);
        Task FillReportIndexAsync(CancellationToken cancellationToken = default);
        Task AddAliasesToIndexAsync(AliasType type, CancellationToken cancellationToken = default);
    }
}