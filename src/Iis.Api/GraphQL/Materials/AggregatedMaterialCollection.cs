using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using Iis.Interfaces.Elastic;

namespace IIS.Core.GraphQL.Materials
{
    public class AggregatedMaterialCollection : ObjectType<(IEnumerable<Material> materials,
        Dictionary<string, AggregationItem> aggregations, int totalCount)>
    {
        protected override void Configure(IObjectTypeDescriptor<(IEnumerable<Material> materials,
            Dictionary<string, AggregationItem> aggregations,
            int totalCount)> descriptor)
        {
            descriptor.Name("AggregatedMaterialCollection");
            descriptor.BindFieldsExplicitly().Include<Resolvers>();
        }

        private class Resolvers
        {
            public int Count([Parent] (IEnumerable<Material> materials, Dictionary<string, AggregationItem> aggregations, int totalCount) parent) => parent.totalCount;
            [GraphQLType(typeof(AnyType))]
            public Dictionary<string, AggregationItem> Aggregations([Parent] (IEnumerable<Material> materials, Dictionary<string, AggregationItem> aggregations, int totalCount) parent) => parent.aggregations;
            [GraphQLType(typeof(NonNullType<ListType<NonNullType<ObjectType<Material>>>>))]
            public IEnumerable<Material> Items([Parent] (IEnumerable<Material> materials, Dictionary<string, AggregationItem> aggregations, int totalCount) parent) => parent.materials;
        }
    }
}
