using Iis.Interfaces.Meta;
using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface INodeTypeLinked
    {
        // workaround because of DataGridView bug
        Guid Id { get; }
        string Name { get; }
        string Title { get; }
        string Meta { get; }
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
        bool IsArchived { get; }
        Kind Kind { get; }
        bool IsAbstract { get; }
        string UniqueValueFieldName { get; }
        string IconBase64Body { get; }
        bool IsHierarchyParent { get; }
        SecurityStrategy SecurityStrategy { get; }
        bool HasUniqueValues { get; }
        Type ClrType { get; }
        bool HasInversed { get; }
        bool IsInversed { get; }
        bool Hidden { get; }
        void SetIsInversed();
        ISchemaMeta MetaObject { get; }
        IReadOnlyList<IRelationTypeLinked> IncomingRelations { get; }
        IReadOnlyList<IRelationTypeLinked> OutgoingRelations { get; }
        INodeTypeLinked AttributeTypeModel { get; }
        IAttributeType  AttributeType { get; }
        IRelationTypeLinked RelationType { get; }
        string GetMetaDeep();
        IReadOnlyList<IChildNodeType> GetDirectChildren(bool setInheritedFrom);
        IReadOnlyList<IChildNodeType> GetAllChildren();
        IReadOnlyList<INodeTypeLinked> GetDirectAncestors();
        IReadOnlyList<INodeTypeLinked> DirectParents => GetDirectAncestors();
        IReadOnlyList<INodeTypeLinked> GetAllAncestors();
        IReadOnlyList<INodeTypeLinked> AllParents => GetAllAncestors();
        IReadOnlyList<INodeTypeLinked> GetDirectDescendants();
        IReadOnlyList<INodeTypeLinked> GetAllDescendants();
        IReadOnlyList<INodeTypeLinked> GetNodeTypesThatEmbedded();
        IReadOnlyList<INodeTypeLinked> GetDirectProperties();
        IReadOnlyList<INodeTypeLinked> DirectProperties => GetDirectProperties();
        IReadOnlyList<INodeTypeLinked> GetAllProperties();
        IEnumerable<INodeTypeLinked> AllProperties => GetAllProperties();
        IRelationTypeLinked GetRelationByName(string relationName);
        bool IsIdentical(INodeTypeLinked nodeType);
        string GetStringCode();
        Dictionary<string, string> GetPropertiesDict();
        INodeTypeLinked GetProperty(string relationName);
        IReadOnlyList<ISchemaCompareDiffInfo> GetDifference(INodeTypeLinked nodeType);
        bool IsInheritedFrom(string nodeTypeName);
        bool IsObject { get; }
        bool IsObjectOfStudy { get; }
        bool IsEvent { get; }
        bool IsObjectSign { get; }
        bool IsEnum { get; }
        bool IsSeparateObject { get; }
        bool IsLinkFromEventToObjectOfStudy { get; }
        INodeTypeLinked GetNodeTypeByDotNameParts(string[] dotNameParts);
        IRelationTypeLinked GetRelationTypeByName(string name);
        IOntologySchema Schema { get; }
        IReadOnlyList<IRelationTypeLinked> GetComputedRelationTypes();
        bool IsComputed { get; }
        string Formula { get; }
        string TitleAttributeName { get; }
        ScalarType ScalarTypeEnum { get; }
        INodeTypeLinked EntityType { get; }
        bool IsAttributeType { get; }
        bool IsEntityType { get; }
        INodeTypeLinked TargetType { get; }
        bool IsMultiple { get; }
        bool IsRequired { get; }
        bool IsSubtypeOf(INodeTypeLinked type);
        bool AcceptsScalar(object value);
        bool AcceptsOperation(EntityOperation create);
        string GetIconName();
        SecurityStrategy? GetSecurityStrategy();
    }

    public class NodeAggregationInfo
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsAggregated { get; set; }
    }
}
