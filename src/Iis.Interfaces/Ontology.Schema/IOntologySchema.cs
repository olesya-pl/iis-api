using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IOntologySchema: ISchemaEntityTypeFinder, IFieldToAliasMapper
    {
        IOntologySchemaSource SchemaSource { get; }
        void Initialize(IOntologyRawData ontologyRawData);
        IEnumerable<INodeTypeLinked> GetTypes(IGetTypesFilter filter);
        Dictionary<string, INodeTypeLinked> GetFullHierarchyNodes();
        IEnumerable<INodeTypeLinked> GetEntityTypes();
        INodeTypeLinked GetNodeTypeById(Guid id);
        IOntologyRawData GetRawData();
        void SetEmbeddingOptions(string entityName, string relationName, EmbeddingOptions embeddingOptions);
        void SetRelationMeta(string entityName, string relationName, string meta);
        ISchemaCompareResult CompareTo(IOntologySchema schema);
        Dictionary<string, INodeTypeLinked> GetStringCodes();
        INodeTypeLinked GetNodeTypeByStringCode(string code);
        INodeTypeLinked UpdateNodeType(INodeTypeUpdateParameter updateParameter);
        INodeTypeLinked CreateEntityType(string name, string title = null, bool isAbstract = false, Guid? ancestorId = null);
        INodeTypeLinked CreateAttributeType(
            Guid parentId,
            string name,
            string title = null,
            ScalarType scalarType = ScalarType.String,
            EmbeddingOptions embeddingOptions = EmbeddingOptions.Optional,
            ISchemaMeta meta = null);
        INodeTypeLinked CreateAttributeTypeJson(
            Guid parentId,
            string name,
            string title = null,
            ScalarType scalarType = ScalarType.String,
            EmbeddingOptions embeddingOptions = EmbeddingOptions.Optional,
            string jsonMeta = null);
        INodeTypeLinked CreateRelationType(
            Guid sourceId,
            Guid targetId,
            string name,
            string title = null,
            EmbeddingOptions embeddingOptions = EmbeddingOptions.Optional,
            ISchemaMeta meta = null);
        INodeTypeLinked CreateRelationTypeJson(
            Guid sourceId,
            Guid targetId,
            string name,
            string title = null,
            EmbeddingOptions embeddingOptions = EmbeddingOptions.Optional,
            string jsonMeta = null);
        void UpdateTargetType(Guid relationTypeId, Guid targetTypeId);
        void SetInheritance(Guid sourceTypeId, Guid targetTypeId);
        void RemoveInheritance(Guid sourceTypeId, Guid targetTypeId);
        IAttributeInfoList GetAttributesInfo(string entityName);
        IAttributeInfoList GetHistoricalAttributesInfo(string entityName, string historicalEntityName);
        IAliases Aliases { get; }
        void RemoveRelation(Guid relationId);
        IEnumerable<INodeTypeLinked> GetAllNodeTypes();
        void PutInOrder();
        void RemoveEntity(Guid id);
        string ValidateRemoveEntity(Guid id);
        bool IsFuzzyDateEntity(INodeTypeLinked nodeType);
        bool IsFuzzyDateEntityAttribute(INodeTypeLinked nodeType);
        IDotName GetDotName(string value);
        IReadOnlyList<INodeTypeLinked> GetNodeTypes(IEnumerable<Guid> ids, bool includeChildren = false);
        IReadOnlyList<INodeTypeLinked> GetEntityTypesByName(IEnumerable<string> names, bool includeChildren);
    }
}
