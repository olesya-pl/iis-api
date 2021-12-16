using System;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.Common
{
    public class NullableDateRangeFilter
    {
        [GraphQLType(typeof(DateTimeType))]
        public DateTime? From { get; set; }
        [GraphQLType(typeof(DateTimeType))]
        public DateTime? To { get; set; }
    }
}
