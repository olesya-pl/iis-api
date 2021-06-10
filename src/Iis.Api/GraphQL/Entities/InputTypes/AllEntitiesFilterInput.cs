using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using Iis.Interfaces.Elastic;

namespace IIS.Core.GraphQL.Entities.InputTypes
{
    public class AllEntitiesFilterInput : FilterInput
    {
        [GraphQLType(typeof(ListType<NonNullType<StringType>>))] 
        public IEnumerable<string> Types { get; set; }
        public IEnumerable<CherryPickedItem> CherryPickedItems { get; set; } = new List<CherryPickedItem>();
        public List<Property> FilteredItems { get; set; } = new List<Property>();        
    }

    public class CherryPickedItem
    {
        public string Id { get; set; }
        public bool IncludeDescendants { get; set; }
    }
}
