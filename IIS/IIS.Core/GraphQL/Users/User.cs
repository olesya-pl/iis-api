using System;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Users
{
    public class User
    {
        [GraphQLType(typeof(IdType))]              public Guid   Id        { get; set; }
        [GraphQLNonNullType]                       public string Name      { get; set; }
        [GraphQLNonNullType]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use latin letters only please")]
                                                   public string Username  { get; set; }
                                                   public bool   IsBlocked { get; set; }
        [GraphQLNonNullType]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must have at least 6 symbols, maximum length is 100")]
                                                    public string Password  { get; set; }
        public User() { }

        public User(IIS.Core.Users.EntityFramework.User dbUser)
        {
            if (dbUser is null)
            {
                throw new ArgumentNullException(nameof(dbUser));
            }

            Id        = dbUser.Id;
            Name      = dbUser.Name;
            Username  = dbUser.UserName;
            IsBlocked = dbUser.IsBlocked;
        }

        public User(UserUpdateInput userUpdateInput, string password, string userName)
        {
            if (userUpdateInput is null)
            {
                throw new ArgumentNullException(nameof(userUpdateInput));
            }

            Id        = userUpdateInput.Id;
            Name      = userUpdateInput.Name;
            IsBlocked = userUpdateInput.IsBlocked;
            Password  = password ?? throw new ArgumentNullException(nameof(password));
            Username  = userName ?? throw new ArgumentNullException(nameof(userName));
        }
    }
}
