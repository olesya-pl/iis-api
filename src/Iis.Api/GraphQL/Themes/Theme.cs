using System;
using IIS.Core.GraphQL.Users;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Themes
{
    public class Theme
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public int QueryResults { get; set; }
        public int ReadQueryResults { get; set; }
        public string Title { get; set; }
        public string QueryRequest { get; set; }
        public User User { get; set; }
        public ThemeType Type { get; set; }
        public string Comment { get; set; }
        public string Meta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}