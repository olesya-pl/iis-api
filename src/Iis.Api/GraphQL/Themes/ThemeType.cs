using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Themes
{
    public class ThemeType
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string ShortTitle { get; set; }
        public string Title { get; set; }
        public string EntityTypeName { get; set; }
    }
}