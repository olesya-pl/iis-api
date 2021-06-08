using System;
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;

namespace IIS.Core.GraphQL.Entities.InputTypes
{
    public class FilterInput
    {
        public string SearchQuery { get; set; }
        public string Suggestion { get; set; }
        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public IEnumerable<Guid> MatchList { get; set; }
    }
}
