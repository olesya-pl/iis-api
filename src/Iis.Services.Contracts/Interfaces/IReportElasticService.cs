using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Params;
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
        Task<(int Count, List<ReportDto> Items)> SearchAsync(ReportSearchParams search, User user);
        Task<int> CountAsync(ReportSearchParams search);
        Task<ReportDto> GetAsync(Guid id);
    }
}