using System;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Users
{
    public class User
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Name { get; set; }

        [GraphQLNonNullType]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use latin letters only please")]
        public string Username  { get; set; }
        public bool IsBlocked { get; set; }

        public User(IIS.Core.Users.EntityFramework.User dbUser)
        {
            Id        = dbUser.Id.ToString();
            Name      = dbUser.Name;
            Username  = dbUser.Username;
            IsBlocked = dbUser.IsBlocked;
        }
    }
}
