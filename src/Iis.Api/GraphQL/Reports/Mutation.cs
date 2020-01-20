using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.DataLoaders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Reports;

namespace IIS.Core.GraphQL.Reports
{
    public class Mutation
    {
        public async Task<Report> CreateReport([Service] OntologyContext context, [GraphQLNonNullType] ReportInput data)
        {
            var report = new Iis.DataModel.Reports.ReportEntity
            {
                Id        = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Recipient = data.Recipient,
                Title     = data.Title
            };

            context.Reports.Add(report);
            await context.SaveChangesAsync();

            return new Report(report);
        }

        public async Task<Report> UpdateReport([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id, [GraphQLNonNullType] ReportInput data)
        {
            var reportInDb = context.Reports.Find(id);
            if (reportInDb == null)
                throw new InvalidOperationException($"Cannot find report with id = {id}");
            reportInDb.Recipient = data.Recipient;
            reportInDb.Title     = data.Title;
            await context.SaveChangesAsync();
            return new Report(reportInDb);
        }

        public async Task<DeleteEntityReportResponse> DeleteReport([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var report = context.Reports.Include(r => r.ReportEvents).Single(r => r.Id == id);
            context.Reports.Remove(report);
            await context.SaveChangesAsync();
            return new DeleteEntityReportResponse
            {
                Details = new Report(report)
            };
        }

        public async Task<Report> UpdateReportEvents([Service]                                  IResolverContext  ctx,
                                                     [Service]                                  OntologyContext   context,
                                                     [GraphQLType(typeof(NonNullType<IdType>))] Guid              id,
                                                     [GraphQLNonNullType]                       UpdateReportData  data)
        {
            var forAdd    = new HashSet<Guid>(data.AddEvents);
            var forRemove = new HashSet<Guid>(data.RemoveEvents);

            forAdd   .ExceptWith(data.RemoveEvents);
            forRemove.ExceptWith(data.AddEvents);

            var loader =  ctx.DataLoader<NodeDataLoader>();
            var addRange = forAdd.Select(eventId => new ReportEventEntity
            {
                EventId = eventId,
                ReportId = id
            }).ToList();
            var removeRange = context.ReportEvents.Where(re => re.ReportId == id && forRemove.Contains(re.EventId))
                .ToList();

            if (addRange.Count > 0 || removeRange.Count > 0)
            {
                if (addRange.Count > 0)
                    context.ReportEvents.AddRange(addRange);
                if (removeRange.Count > 0)
                    context.ReportEvents.RemoveRange(removeRange);
                await context.SaveChangesAsync();
            }

            var report = await context.Reports
                .Include(r => r.ReportEvents)
                .SingleOrDefaultAsync(r => r.Id == id);

            if (report == null)
                throw new InvalidOperationException($"Cannot find report with id = {id}");

            return new Report(report);
        }

        public async Task<Report> CopyReport([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id, [GraphQLNonNullType] CopyReportInput data)
        {
            var existingReport = context.Reports.Include(r => r.ReportEvents).SingleOrDefault(u => u.Id == id);
            if (existingReport == null)
                throw new InvalidOperationException($"Cannot find report with id  = {id}");

            var newReport = new Iis.DataModel.Reports.ReportEntity(existingReport, Guid.NewGuid(), DateTime.Now);

            newReport.Title = data.Title ?? newReport.Title;
            newReport.Recipient = data.Recipient ?? newReport.Recipient;

            context.Reports.Add(newReport);
            await context.SaveChangesAsync();
            return new Report(newReport);
        }
    }
}
