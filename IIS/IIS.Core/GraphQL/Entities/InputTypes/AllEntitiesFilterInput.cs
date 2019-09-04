using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities.InputTypes
{
    public class AllEntitiesFilterInput : FilterInput
    {
        [GraphQLType(typeof(ListType<NonNullType<StringType>>))] public IEnumerable<string> Types { get; set; }
    }
}
