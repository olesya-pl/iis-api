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
        string Title { get; set; }
        IMeta Meta { get; set; }
        JObject MetaSource { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        bool HasUniqueValues { get; }
        string UniqueValueFieldName { get; set; }
        Type ClrType { get; }
        IEnumerable<IEntityTypeModel> DirectParents { get; }
        IEnumerable<IEntityTypeModel> AllParents { get; }
        IEnumerable<IEmbeddingRelationTypeModel> DirectProperties { get; }
        IEnumerable<IEmbeddingRelationTypeModel> AllProperties { get; }
        bool IsObjectOfStudy { get; }
        INodeTypeLinked Source { get; }
        
        
        void AddType(INodeTypeModel type);
        IEmbeddingRelationTypeModel GetProperty(string typeName);
        bool IsSubtypeOf(INodeTypeModel type);
        string ToString();
    }
}