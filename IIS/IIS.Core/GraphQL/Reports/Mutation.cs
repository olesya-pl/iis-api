using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.DataLoaders;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Reports
{
    public class Mutation
    {
        public async Task<Report> CreateReport([Service] OntologyContext context, [GraphQLNonNullType] ReportInput data)
        {
            var report = new Core.Report.EntityFramework.Report
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
            context.ReportEvents.AddRange(forAdd.Select(eventId => new Core.Report.EntityFramework.ReportEvents
            {
                EventId  = eventId,
                ReportId = id
            }));

            context.ReportEvents.RemoveRange(context.ReportEvents.Where(re => re.ReportId == id && forRemove.Contains(re.EventId)));
            await context.SaveChangesAsync();

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

            var newReport = new Core.Report.EntityFramework.Report(existingReport, Guid.NewGuid(), DateTime.Now);

            newReport.Title = data.Title ?? newReport.Title;
            newReport.Recipient = data.Recipient ?? newReport.Recipient;

            context.Reports.Add(newReport);
            await context.SaveChangesAsync();
            return new Report(newReport);
        }
    }
}
