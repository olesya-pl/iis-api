using HotChocolate;
using HotChocolate.Types;
using IIS.Core.Ontology.EntityFramework.Context;
using System;
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

            return new Report(report.Id, report.Title, report.Recipient, report.CreatedAt);
        }

        public async Task<Report> UpdateReport([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid reportId, [GraphQLNonNullType] ReportInput data)
        {
            var reportInDb = context.Reports.Find(reportId);
            if (reportInDb == null)
                throw new InvalidOperationException($"Cannot find report with id = {reportId}");
            reportInDb.Recipient = data.Recipient;
            reportInDb.Title     = data.Title;
            await context.SaveChangesAsync();
            return new Report(reportInDb.Id, reportInDb.Title, reportInDb.Recipient, reportInDb.CreatedAt);
        }

        public async Task DeleteReport([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid reportId)
        {
            var report = new Core.Report.EntityFramework.Report { Id = reportId };
            context.Reports.Attach(report);
            context.Reports.Remove(report);
            context.SaveChanges();
        }
    }
}
