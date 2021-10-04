using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Roles;
using Iis.Interfaces.Enums;
using Iis.Interfaces.Users;

namespace IIS.Core.GraphQL.Users
{
    /// <summary>
    /// Represents output model for User 
    /// </summary>
    public class User
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
        public int AccessLevel { get; set; }
        public UserSource Source { get; set; }
        public bool IsExternalUser => Source != UserSource.Internal;
        public IEnumerable<Role> Roles { get; set; }
        public IEnumerable<AccessEntity> Entities { get; set; }
        public IEnumerable<AccessTab> Tabs { get; set; }
    }
}
