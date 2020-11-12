using System;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Themes
{
    public class SetThemeReadCountInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public int ReadCount { get; set; }
    }
}
