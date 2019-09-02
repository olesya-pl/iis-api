using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;

namespace IIS.Core.GraphQL.Users
{
    public class User
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid Id { get; set; }
        [GraphQLNonNullType] public string Name { get; set; }
        [GraphQLNonNullType]public string Username { get; set; }
        public bool IsBlocked { get; set; }
        [GraphQLType(typeof(NotImplementedType))] public string Role { get; set; }
    }
}
