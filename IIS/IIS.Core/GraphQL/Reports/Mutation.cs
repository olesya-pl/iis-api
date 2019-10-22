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

        public async Task<Report> UpdateReport([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid reportId, [GraphQLNonNullType] ReportInput data)
        {
            var reportInDb = context.Reports.Find(reportId);
            if (reportInDb == null)
                throw new InvalidOperationException($"Cannot find report with id = {reportId}");
            reportInDb.Recipient = data.Recipient;
            reportInDb.Title     = data.Title;
            await context.SaveChangesAsync();
            return new Report(reportInDb);
        }

        public async Task<int> DeleteReport([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid reportId)
        {
            var report = new Core.Report.EntityFramework.Report { Id = reportId };
            context.Reports.Attach(report);
            context.Reports.Remove(report);
            return await context.SaveChangesAsync();
        }

        public async Task<Report> UpdateReportEvents([Service]                                  IResolverContext  ctx,
                                                     [Service]                                  OntologyContext   context,
                                                     [GraphQLType(typeof(NonNullType<IdType>))] Guid              reportId,
                                                     [GraphQLNonNullType]                       UpdateReportData  data)
        {
            var forAdd    = new HashSet<Guid>(data.AddEvents);
            var forRemove = new HashSet<Guid>(data.RemoveEvents);

            forAdd   .ExceptWith(data.RemoveEvents);
            forRemove.ExceptWith(data.AddEvents);

            var loader =  ctx.DataLoader<NodeDataLoader>();
            context.ReportEvents.AddRange(forAdd.Select(id => new Core.Report.EntityFramework.ReportEvents
            {
                EventId  = id,
                ReportId = reportId
            }));

            context.ReportEvents.RemoveRange(context.ReportEvents.Where(re => re.ReportId == reportId && forRemove.Contains(re.EventId)));
            await context.SaveChangesAsync();

            var report = await context.Reports
                .Include(r => r.ReportEvents)
                .SingleOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                throw new InvalidOperationException($"Cannot find report with id = {reportId}");

            return new Report(report);
        }
    }
}
