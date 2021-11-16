using System;
using HotChocolate;
using HotChocolate.Types;

namespace Iis.Api.GraphQL.Common
{
    public class NullableDateRangeFilter
    {
        [GraphQLType(typeof(DateType))]
        public DateTime? From { get; set; }
        [GraphQLType(typeof(DateType))]
        public DateTime? To { get; set; }
    }
}
