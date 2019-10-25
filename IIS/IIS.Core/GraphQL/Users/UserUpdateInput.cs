using System;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Users
{
    public class UserUpdateInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid   Id        { get; set; }
        [GraphQLNonNullType]                       public string Name      { get; set; }
                                                   public bool   IsBlocked { get; set; }
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must have at least 6 symbols, maximum length is 100")]
                                                   public string Password  { get; set; }
    }
}
