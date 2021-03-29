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
            return (From: new DateTime(From.Year, From.Month, From.Day, 00, 00, 00), To: new DateTime(To.Year, To.Month, To.Day, 23, 59, 59));
        }
    }
}