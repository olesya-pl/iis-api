using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using System.Collections.Generic;

namespace IIS.Core.GraphQL.Entities.InputTypes
{
    public class FilterInput
    {
        public string SearchQuery { get; set; }

        public string Suggestion { get; set; }

        [GraphQLType(typeof(NotImplementedType))]
        public string Search { get; set; } // ConditionPredicate
        [GraphQLType(typeof(ListType<NonNullType<IdType>>))] public IEnumerable<Guid> MatchList { get; set; }
    }
}
