using System;
using System.Collections.Generic;
using HotChocolate;
using IIS.Core.GraphQL.Roles;

namespace Iis.Api.GraphQL.Roles
{
    public class CreateRoleModel
    {
        [GraphQLNonNullType]
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<AccessEntity> Entities { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
        public IEnumerable<Guid> ActiveDirectoryGroupIds { get; set; } 
    }
}
