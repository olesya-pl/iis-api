using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Users
{
    /// <summary>
    /// Represents base input model for User 
    /// </summary>
    public abstract class BaseUserInput
    {
        [GraphQLNonNullType]
        public string LastName { get; set; }
        [GraphQLNonNullType]
        public string FirstName { get; set; }
        [GraphQLNonNullType]
        public string Patronymic { get; set; }
        public string Comment { get; set; }
        [GraphQLNonNullType]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Use only latin letters and numbers please")]
        public string UserName { get; set; }
        public string UserNameActiveDirectory { get; set; }
        public bool? IsBlocked { get; set; }
        [GraphQLNonNullType]
        public IEnumerable<Guid> Roles { get; set; }
    }

    /// <summary>
    /// Represents input model for User in case of Create
    /// </summary>
    public class UserCreateInput : BaseUserInput
    {
        [GraphQLNonNullType]
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }

    /// <summary>
    /// Represents input model for User in case of Update
    /// </summary>
    public class UserUpdateInput: BaseUserInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
