using System.Collections.Generic;
using HotChocolate.Types;

namespace IIS.Core.GraphQL
{
    public interface IGraphQlTypeProvider
    {
        Dictionary<string, ObjectType> OntologyTypes { get; }
        Dictionary<string, HotChocolate.Types.ScalarType> Scalars { get; }
    }

    public class GraphQlTypeProvider : IGraphQlTypeProvider
    {
        public Dictionary<string, ObjectType> OntologyTypes { get; } = new Dictionary<string, ObjectType>();

        // TODO: Use ScalarType Enum, fill with all scalar types
        public Dictionary<string, HotChocolate.Types.ScalarType> Scalars { get; } = new Dictionary<string, HotChocolate.Types.ScalarType>
        {
            ["string"] = new StringType(),
            ["int"] = new IntType(),
        };
    }
}