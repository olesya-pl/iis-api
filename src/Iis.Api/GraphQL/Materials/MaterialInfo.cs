using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialInfo
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string SourceType { get; set; }
        public string SourceVersion { get; set; }
        public IEnumerable<MaterialFeature> Features { get; } = new List<MaterialFeature>();
    }
}
