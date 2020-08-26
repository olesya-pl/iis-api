using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Roles;

namespace Iis.Api.GraphQL.Roles
{
    public class UpdateRoleModel
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Name { get; set; }
        public string Description { get; set; }
        public string AdGroup { get; set; }
        public IEnumerable<AccessEntity> Entities { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
        public IEnumerable<Guid> ActiveDirectoryGroupIds { get; set; }
    }
}
