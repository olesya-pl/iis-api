using System;
using System.ComponentModel.DataAnnotations;

using HotChocolate;

namespace IIS.Core.GraphQL.Themes
{
    public class ThemeInput
    {
        [GraphQLNonNullType]
        [Required]
        public string Query {get;set;}
        [GraphQLNonNullType]
        [Required]
        public Guid? UserId {get;set;}
        [GraphQLNonNullType]
        [Required]
        public Guid? ThemeTypeId {get;set;}
    }
}