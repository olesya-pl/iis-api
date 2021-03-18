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
        [GraphQLType(typeof(ListType<NonNullType<StringType>>))] 
        public IEnumerable<string> CherryPickedItems { get; set; } = new List<string>();
        public List<Property> FilteredItems { get; set; } = new List<Property>();
        
    }
}
