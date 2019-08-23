using HotChocolate;
using IIS.Core.GraphQL.Common;

namespace IIS.Core.GraphQL.Entities.InputTypes
{
    public class FilterInput
    {
        [GraphQLNonNullType] public string SearchQuery { get; set; }

        [GraphQLNonNullType] public string Suggestion { get; set; }

        [GraphQLType(typeof(NotImplementedType))]
        public string Search { get; set; } // ConditionPredicate
    }
}
