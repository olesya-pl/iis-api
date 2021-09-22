using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;
using Iis.Interfaces.Enums;

namespace IIS.Core.GraphQL.Users
{
    /// <summary>
    /// Represents base input model for User 
    /// </summary>
    public abstract class BaseUserInput
    {
        public string Comment { get; set; }
        [GraphQLNonNullType]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Use only latin letters, numbers and symbols '_', '-' please")]
        public string UserName { get; set; }
        [Obsolete]
        public string UserNameActiveDirectory { get; set; }
        public bool? IsBlocked { get; set; }
        [GraphQLNonNullType]
        public int AccessLevel { get; set; }
        [GraphQLNonNullType]
        public IEnumerable<Guid> Roles { get; set; }
    }

    /// <summary>
    /// Represents input model for User in case of Create
    /// </summary>
    public class UserCreateInput : BaseUserInput
    {
        [GraphQLNonNullType]
        public string LastName { get; set; }
        [GraphQLNonNullType]
        public string FirstName { get; set; }
        [GraphQLNonNullType]
        public string Patronymic { get; set; }
        [GraphQLNonNullType]
        [Required, DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "{0} must have at least {2} symbols, maximum length is {1}")]
        [RegularExpression(@"^[a-zA-Z0-9_=-]+$", ErrorMessage = "Use only latin letters, numbers and symbols '_' '=' '-' please")]
        public string Password { get; set; }
    }

    /// <summary>
    /// Represents input model for User in case of Update
    /// </summary>
    public class UserUpdateInput : BaseUserInput
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        [DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "{0} must have at least {2} symbols, maximum length is {1}")]
        [RegularExpression(@"^[a-zA-Z0-9_=-]+$", ErrorMessage = "Use only latin letters, numbers and symbols '_' '=' '-' please")]
        public string Password { get; set; }
        public bool IsExternalUser { get; set; }
    }
}
