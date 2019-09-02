using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Files;
using IIS.Core.GraphQL.Scalars;

namespace IIS.Core.GraphQL.Materials
{
    public class Material
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid Id { get; set; }
        [GraphQLNonNullType] public IEnumerable<Material> Children { get; set; }
        public FileInfo File { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
    }
}
