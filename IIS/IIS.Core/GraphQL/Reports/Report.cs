using HotChocolate;
using HotChocolate.Types;
using System;

namespace IIS.Core.GraphQL.Reports
{
    public class Report
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid Id            { get; set; }
        [GraphQLNonNullType]                       public string Title       { get; set; }
        [GraphQLNonNullType]                       public string Recipient   { get; set; }
        [GraphQLNonNullType]                       public DateTime CreatedAt { get; set; }

        public Report(Guid id, string title, string recepient, DateTime createdAt)
        {
            Id        = id;
            Title     = title     ?? throw new ArgumentNullException(nameof(title));
            Recipient = recepient ?? throw new ArgumentNullException(nameof(recepient));
            CreatedAt = createdAt;
        }
    }
}
