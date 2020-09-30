using Iis.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IReportService
    {
        Task<ReportDto> CopyAsync(Guid sourceId, ReportDto newReport);
        Task<ReportDto> CreateAsync(ReportDto report);
        Task<(int Count, List<ReportDto> Items)> GetReportPageAsync(int size, int offset);
        Task<ReportDto> RemoveAsync(Guid id);
        Task<ReportDto> UpdateAsync(ReportDto report);
        Task<ReportDto> UpdateEventsAsync(Guid id, IEnumerable<Guid> eventIdsToAdd, IEnumerable<Guid> eventIdsToRemove);
        Task<ReportDto> GetAsync(Guid id);
    }
}