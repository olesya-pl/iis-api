using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Materials;
using System;
using IIS.Core.GraphQL.Common;

namespace IIS.Core.GraphQL.Reports
{
    public class RelatedMaterialsItem
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid EventId {get; set;}
        public GraphQLCollection<Material> Materials {get;set;}
    }
}
