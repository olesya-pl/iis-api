using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;

using System.ComponentModel.DataAnnotations;

namespace IIS.Core.GraphQL.Users
{
    /// <summary>
    /// Represents input model for User (uses for Insert/Update) 
    /// </summary>
    public class User2Input
    {
        public Guid? Id { get; set; }
        [GraphQLNonNullType]
        public string LastName { get; set; }
        [GraphQLNonNullType]
        public string FirstName { get; set; }
        [GraphQLNonNullType]
        public string Patronymic { get; set; }
        public string Comment { get; set; }
        [GraphQLNonNullType]
        public string UserName { get; set; }
        [GraphQLNonNullType]
        public string Password { get; set; }
        public string UserNameActiveDirectory { get; set; }
        [GraphQLNonNullType]
        public IEnumerable<Guid> Roles { get; set; }
    }

    public class UserInput
    {
        public string Name { get; set; }

        public string Username { get; set; }

        public bool? IsBlocked { get; set; }

        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must have at least 6 symbols, maximum length is 100")]
        public string Password { get; set; }
    }
}
