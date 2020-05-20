using System;
using IIS.Core.GraphQL.Users;

using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Themes
{
    public class Theme
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id {get;set;}
        public int QueryResults {get;set;}
        public string Title {get;set;}
        public string Query {get;set;}
        public User User {get;set;}
        public ThemeType Type {get;set;}
    }
}