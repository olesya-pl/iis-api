using HotChocolate;
using HotChocolate.Types;
using Iis.Interfaces.Enums;
using System;

namespace Iis.Api.GraphQL.Aliases
{
    public class Alias
    {
        [GraphQLType(typeof(IdType))]
        public Guid Id { get; set; }
        public string DotName { get; set; }
        public string Value { get; set; }
        public AliasType Type { get; set; }
    }
}
