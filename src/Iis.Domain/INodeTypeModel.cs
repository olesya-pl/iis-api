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
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
        bool HasUniqueValues { get; }
        string UniqueValueFieldName { get; }
        bool IsAbstract { get; }
        bool IsComputed { get; }
        IEnumerable<INodeTypeModel> DirectParents { get; }
        IEnumerable<INodeTypeModel> AllParents { get; }
        IEnumerable<INodeTypeModel> DirectProperties { get; }
        IEnumerable<INodeTypeModel> AllProperties { get; }
        bool IsObjectOfStudy { get; }
        INodeTypeLinked Source { get; }
        INodeTypeModel GetProperty(string typeName);
        bool IsSubtypeOf(INodeTypeModel type);

        INodeTypeModel AttributeTypeModel { get; }
        IAttributeType AttributeType { get; }
        IRelationTypeLinked RelationType { get; }
        
        #region AttributeType
        ScalarType ScalarTypeEnum { get; }
        bool AcceptsScalar(object value);
        #endregion

        #region RelationType
        EmbeddingOptions EmbeddingOptions { get; }
        INodeTypeModel EntityType { get; }
        bool IsAttributeType { get; }
        bool IsEntityType { get; }
        bool IsInversed { get; }
        INodeTypeModel TargetType { get; }
        bool AcceptsOperation(EntityOperation create);
        #endregion
    }
}