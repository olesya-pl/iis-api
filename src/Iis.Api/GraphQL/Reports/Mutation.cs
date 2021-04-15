using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Reports
{
    public class Mutation
    {
        public async Task<Report> CreateReport([Service] IReportService reportService, [GraphQLNonNullType] ReportInput data)
        {
            var report = await reportService.CreateAsync(new ReportDto
            {
                Recipient = data.Recipient,
                Title = data.Title,
                AccessLevel = data.AccessLevel
            });

            return new Report(report);
        }

        public async Task<Report> UpdateReport([Service] IReportService reportService, [GraphQLType(typeof(NonNullType<IdType>))] Guid id, [GraphQLNonNullType] ReportInput data)
        {
            var updatedReport = await reportService.UpdateAsync(new ReportDto 
            {
                Id = id,
                Title = data.Title,
                Recipient = data.Recipient,
                AccessLevel = data.AccessLevel
            });

            return new Report(updatedReport);
        }

        public async Task<DeleteEntityReportResponse> DeleteReport([Service] IReportService reportService, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var removedReport = await reportService.RemoveAsync(id);
            return new DeleteEntityReportResponse
            {
                Details = new Report(removedReport)
            };
        }

        public async Task<Report> UpdateReportEvents([Service] IResolverContext ctx,
                                                     [Service] IReportService reportService,
                                                     [GraphQLType(typeof(NonNullType<IdType>))] Guid id,
                                                     [GraphQLNonNullType] UpdateReportData data)
        {
            var updatedReport = await reportService.UpdateEventsAsync(id, data.AddEvents, data.RemoveEvents);

            return new Report(updatedReport);
        }

        public async Task<Report> CopyReport([Service] IReportService reportService, [GraphQLType(typeof(NonNullType<IdType>))] Guid id, [GraphQLNonNullType] CopyReportInput data)
        {
            var copiedReport = await reportService.CopyAsync(id, new ReportDto 
            {
                Title = data.Title,
                Recipient = data.Recipient,
                AccessLevel = data.AccessLevel
            });
            return new Report(copiedReport);
        }
    }
}
