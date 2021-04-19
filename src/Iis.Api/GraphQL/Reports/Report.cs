using HotChocolate;
using HotChocolate.Types;
using Iis.Services.Contracts.Dtos;
using System;
using System.Linq;

namespace IIS.Core.GraphQL.Reports
{
    public class Report
    {
        [GraphQLType(typeof(NonNullType<IdType>))] 
        public Guid Id { get; set; }
        [GraphQLNonNullType] 
        public string Title { get; set; }
        [GraphQLNonNullType] 
        public string Recipient { get; set; }
        public int AccessLevel { get; set; }
        [GraphQLNonNullType] 
        public DateTime CreatedAt { get; set; }
        [GraphQLIgnore] 
        public Guid[] EventIds { get; set; }
        public string Annotation { get; set; }

        public Report(ReportDto report) : this(report.Id, report.Title, report.Recipient, report.CreatedAt, report.AccessLevel, report.ReportEventIds.ToArray(), report.Annotation) { }

        public Report(Guid id, string title, string recepient, DateTime createdAt, int accessLevel, Guid[] events, string annotation)
        {
            Id = id;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Recipient = recepient ?? throw new ArgumentNullException(nameof(recepient));
            CreatedAt = createdAt;
            AccessLevel = accessLevel;
            EventIds = events;
            Annotation = annotation;
        }
    }
}
