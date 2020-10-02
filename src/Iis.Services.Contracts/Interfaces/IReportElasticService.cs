using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IReportElasticService
    {
        Task<List<ElasticBulkResponse>> PutAsync(IEnumerable<ReportDto> reports);
        Task<bool> PutAsync(ReportDto report);
        Task<bool> RemoveAsync(Guid id);
        Task<(int Count, List<ReportDto> Items)> SearchAsync(int pageSize, int offset, string sortColum, string sortOrder);
        Task<ReportDto> GetAsync(Guid id);
    }
}