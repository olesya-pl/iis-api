using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface INodeTypeModel
    {
        Guid Id { get; }
        string Name { get; }
        string Title { get; }
        IMeta Meta { get; }
        JObject MetaSource { get; }
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
        bool HasUniqueValues { get; }
        string UniqueValueFieldName { get; }
        IEnumerable<IEntityTypeModel> DirectParents { get; }
        IEnumerable<IEntityTypeModel> AllParents { get; }
        IEnumerable<IEmbeddingRelationTypeModel> DirectProperties { get; }
        IEnumerable<IEmbeddingRelationTypeModel> AllProperties { get; }
        bool IsObjectOfStudy { get; }
        INodeTypeLinked Source { get; }
        IEmbeddingRelationTypeModel GetProperty(string typeName);
        bool IsSubtypeOf(INodeTypeModel type);
        string ToString();
    }
}