using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using Iis.Interfaces.Elastic;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialsFilterInput
    {
        public string Suggestion { get; set; }
        [GraphQLType(typeof(ListType<NonNullType<StringType>>))]
        public List<string> CherryPickedItems { get; set; } = new List<string>();
        public List<Property> FilteredItems { get; set; } = new List<Property>();
    }
}
