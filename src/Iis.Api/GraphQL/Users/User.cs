using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Roles;

namespace IIS.Core.GraphQL.Users
{
    /// <summary>
    /// Represents output model for User 
    /// </summary>
    public class User2
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string FullName => $"{LastName} {FirstName} {Patronymic}";
        public string Comment { get; set; }
        public string UserName { get; set; }
        public string UserNameActiveDirectory { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public IEnumerable<AccessEntity> Entities { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
    }
    public class User
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Name { get; set; }

        [GraphQLNonNullType]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use latin letters only please")]
        public string Username { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }
        public IEnumerable<AccessEntity> Entities { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
    }
}
