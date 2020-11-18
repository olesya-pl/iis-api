using System;
using HotChocolate;

namespace IIS.Core.GraphQL.ChangeHistory
{
    public class DateRangeFilter
    {
        [GraphQLNonNullType]
        public DateTime From { get; set; }
        [GraphQLNonNullType]
        public DateTime To { get; set; }
        [GraphQLIgnore]
        public (DateTime From, DateTime To) ToRange()
        {
            var localFrom = From.ToLocalTime();
            var localTo = To.ToLocalTime();

            return (From: new DateTime(localFrom.Year, localFrom.Month, localFrom.Day, 00, 00, 00), To: new DateTime(localTo.Year, localTo.Month, localTo.Day, 23, 59, 59));
        }
    }
}