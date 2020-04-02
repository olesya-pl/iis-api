using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    /// <summary>
    /// Defines and implement type that represent list of Materials and wraps GetMaterials query value tuple response
    /// </summary>
    public class MaterialCollection : ObjectType<(IEnumerable<Material> materials, int totalCount)>
    {
        protected override void Configure(IObjectTypeDescriptor<(IEnumerable<Material>materials, int totalCount)> descriptor)
        {
            descriptor.Name("MaterialCollection");
            descriptor.BindFieldsExplicitly().Include<Resolvers>();
        }

        private class Resolvers
        {
            /// <summary>
            /// Defines total number of rows that potentialy could be returned for query disregarding paggination 
            /// </summary>
            public int Count([Parent] (IEnumerable<Material> materials, int totalCount) parent) => parent.totalCount;
            /// <summary>
            /// Defines list of items that will be returned
            /// </summary>
            [GraphQLType(typeof(NonNullType<ListType<NonNullType<ObjectType<Material>>>>))]
            public IEnumerable<Material> Items([Parent] (IEnumerable<Material> materials, int totalCount) parent) => parent.materials;
        }
    }
}
