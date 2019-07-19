using System.Collections.Generic;
using HotChocolate.Types;

namespace IIS.Core.GraphQL
{
    public interface IGraphQlOntologyTypeProvider
    {
        Dictionary<string, ObjectType> OntologyTypes { get; }
    }

    public class GraphQlOntologyTypeProvider : IGraphQlOntologyTypeProvider
    {
        public Dictionary<string, ObjectType> OntologyTypes { get; } = new Dictionary<string, ObjectType>();
    }
}