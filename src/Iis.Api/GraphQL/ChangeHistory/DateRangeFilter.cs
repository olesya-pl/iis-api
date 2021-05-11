using System;
using HotChocolate;
using HotChocolate.Types;
namespace IIS.Core.GraphQL.ChangeHistory
{
    public class DateRangeFilter
    {
        [GraphQLType(typeof(NonNullType<DateType>))]
        public DateTime From { get; set; }
        [GraphQLType(typeof(NonNullType<DateType>))]
        public DateTime To { get; set; }
        [GraphQLIgnore]
        public (DateTime From, DateTime To) ToRange()
        {
            var dayBeforeTo = To.AddDays(-1);
            return (From: new DateTime(From.Year, From.Month, From.Day, 00, 00, 00), To: new DateTime(dayBeforeTo.Year, dayBeforeTo.Month, dayBeforeTo.Day, 23, 59, 59));
        }
    }
}