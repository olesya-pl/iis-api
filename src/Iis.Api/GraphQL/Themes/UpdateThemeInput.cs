using System;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Themes
{
    public class UpdateThemeInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        [Required]
        public Guid Id { get; set; }
        [GraphQLNonNullType]
        [Required]
        public string Title { get; set; }
        [GraphQLNonNullType]
        [Required]
        public string QueryRequest { get; set; }
        public Guid? UserId { get; set; }
        public string EntityTypeName { get; set; }
        public string Comment { get; set; }
        public string Meta { get; set; }
    }
}
