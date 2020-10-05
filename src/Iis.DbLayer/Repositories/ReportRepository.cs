using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel.Reports;
using IIS.Repository;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Iis.DbLayer.Repositories
{
    public class ReportRepository : RepositoryBase<OntologyContext>, IReportRepository
    {
        public void Create(ReportEntity entity)
        {
            Context.Reports.Add(entity);
        }

        public Task<List<ReportEntity>> GetAllAsync()
        {
            return Context.Reports.Include(x => x.ReportEvents).ToListAsync();
        }

        public Task<ReportEntity> GetByIdAsync(Guid id)
        {
            return Context.Reports.Include(x => x.ReportEvents).SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(int Count, List<ReportEntity> Items)> GetReportPageAsync(int size, int offset)
        {
            var count = await Context.Reports.CountAsync();
            var reports = await Context.Reports
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Take(size)
                .Skip(offset)
                .ToListAsync();

            return (count, reports);
        }

        public void Update(ReportEntity entity)
        {
            Context.Reports.Update(entity);
        }

        public void Remove(ReportEntity entity) 
        {
            Context.Reports.Remove(entity);
        }

        public Task<List<ReportEventEntity>> GetEventsAsync(Guid id, IEnumerable<Guid> eventIds = null)
        {
            var query = Context.ReportEvents.Where(x => x.ReportId == id);

            if (eventIds != null)
                query.Where(x => eventIds.Contains(x.EventId));

            return query.ToListAsync();
        }

        public void AddEvents(IEnumerable<ReportEventEntity> events) 
        {
            Context.ReportEvents.AddRange(events);
        }

        public void RemoveEvents(IEnumerable<ReportEventEntity> events) 
        {
            Context.ReportEvents.RemoveRange(events);
        }
    }
}