using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Roles;

namespace Iis.Api.GraphQL.Roles
{
    public class GroupAccess
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public IEnumerable<AccessEntity> Entities { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
    }
}