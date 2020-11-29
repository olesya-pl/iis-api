using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
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
        ISchemaMeta Meta { get; }
        JObject MetaSource { get; }
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
        bool HasUniqueValues { get; }
        string UniqueValueFieldName { get; }
        bool IsAbstract { get; }
        bool IsComputed { get; }
        IEnumerable<INodeTypeModel> DirectParents { get; }
        IEnumerable<INodeTypeModel> AllParents { get; }
        IEnumerable<IEmbeddingRelationTypeModel> DirectProperties { get; }
        IEnumerable<IEmbeddingRelationTypeModel> AllProperties { get; }
        bool IsObjectOfStudy { get; }
        INodeTypeLinked Source { get; }
        IEmbeddingRelationTypeModel GetProperty(string typeName);
        bool IsSubtypeOf(INodeTypeModel type);
                string ToString();
        
        #region AttributeType
        ScalarType ScalarTypeEnum { get; }
        bool AcceptsScalar(object value);
        #endregion




    }
}