using HotChocolate;
using System;
using System.ComponentModel.DataAnnotations;

namespace IIS.Core.GraphQL.Themes
{
    public class ThemeInput
    {
        [GraphQLNonNullType]
        [Required]
        public string Title { get; set; }
        [GraphQLNonNullType]
        [Required]
        public string QueryRequest { get; set; }
        [GraphQLNonNullType]
        [Required]
        public Guid? UserId { get; set; }
        [GraphQLNonNullType]
        [Required]
        public string EntityTypeName { get; set; }
        public string Meta { get; set; }
    }
}