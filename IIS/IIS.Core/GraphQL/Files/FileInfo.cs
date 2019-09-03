using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Files
{
    public class FileInfo
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public bool IsTemporary { get; set; }
    }
}
