using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;

namespace IIS.Core.GraphQL.Roles
{
    public class Role
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Name { get; set; }
        public string Description { get; set; }

        [GraphQLNonNullType]
        public bool IsAdmin { get; set; }
        public string AdGroup { get; set; }
        public IEnumerable<AccessEntity> Entities { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
    }
}
