using HotChocolate;
using HotChocolate.Types;
using System;
using System.Linq;

namespace IIS.Core.GraphQL.Reports
{
    public class Report
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid              Id        { get; set; }
        [GraphQLNonNullType]                       public string            Title     { get; set; }
        [GraphQLNonNullType]                       public string            Recipient { get; set; }
        [GraphQLNonNullType]                       public DateTime          CreatedAt { get; set; }
        [GraphQLIgnore]                            public Guid[]            EventIds  { get; set; }

        public Report(Iis.DataModel.Reports.ReportEntity report) : this(report.Id, report.Title, report.Recipient, report.CreatedAt, report.ReportEvents.Select(re => re.EventId).ToArray()) { }

        public Report(Guid id, string title, string recepient, DateTime createdAt, Guid[] events)
        {
            Id        = id;
            Title     = title     ?? throw new ArgumentNullException(nameof(title));
            Recipient = recepient ?? throw new ArgumentNullException(nameof(recepient));
            CreatedAt = createdAt;
            EventIds  = events;
        }
    }
}
