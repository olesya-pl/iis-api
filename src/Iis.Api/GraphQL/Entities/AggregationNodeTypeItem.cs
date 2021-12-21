using HotChocolate;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Entities
{
    public class AggregationNodeTypeItem
    {
        [GraphQLType(typeof(IdType))]
        public Guid NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public string Title { get; set; }
        public int DocCount { get; set; }
        public List<AggregationNodeTypeItem> Children { get; set; }

        public override string ToString() => $"{NodeTypeName}: {DocCount}";
    }
}
