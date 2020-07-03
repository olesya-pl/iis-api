using Iis.Interfaces.Meta;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Iis.Domain
{
    public interface INodeTypeModel
    {
        IEnumerable<EntityType> AllParents { get; }
        IEnumerable<IEmbeddingRelationTypeModel> AllProperties { get; }
        Type ClrType { get; }
        DateTime CreatedAt { get; set; }
        IEnumerable<EntityType> DirectParents { get; }
        IEnumerable<IEmbeddingRelationTypeModel> DirectProperties { get; }
        bool HasUniqueValues { get; }
        Guid Id { get; }
        bool IsObjectOfStudy { get; }
        IMeta Meta { get; set; }
        JObject MetaSource { get; set; }
        string Name { get; }
        IEnumerable<INodeTypeModel> RelatedTypes { get; }
        string Title { get; set; }
        string UniqueValueFieldName { get; set; }
        DateTime UpdatedAt { get; set; }

        void AddType(INodeTypeModel type);
        IEmbeddingRelationTypeModel GetProperty(string typeName);
        bool IsSubtypeOf(INodeTypeModel type);
        string ToString();
    }
}