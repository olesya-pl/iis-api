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
        INodeTypeLinked UpdateNodeType(INodeTypeUpdateParameter updateParameter);
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
    }
}
