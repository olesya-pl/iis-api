using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel.Reports;

namespace Iis.DbLayer.Repositories
{
    public interface IReportRepository 
    {
        Task<List<ReportEntity>> GetAllAsync();

        Task<(int Count, List<ReportEntity> Items)> GetReportPageAsync(int size, int page);

        Task<ReportEntity> GetByIdAsync(Guid id);

        void Create(ReportEntity entity);

        void Update(ReportEntity entity);

        void Remove(ReportEntity entity);

        Task<List<ReportEventEntity>> GetEventsAsync(Guid id, IEnumerable<Guid> eventIds = null);

        void AddEvents(IEnumerable<ReportEventEntity> events);

        void RemoveEvents(IEnumerable<ReportEventEntity> events);
    }
}