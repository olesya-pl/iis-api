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
        bool HasUniqueValues { get; }
        Type ClrType { get; }
        bool HasInversed { get; }
        bool IsInversed { get; }
        void SetIsInversed();
        ISchemaMeta MetaObject { get; }
        IReadOnlyList<IRelationTypeLinked> IncomingRelations { get; }
        IReadOnlyList<IRelationTypeLinked> OutgoingRelations { get; }
        IAttributeType AttributeType { get; }
        IRelationTypeLinked RelationType { get; }
        string GetMetaDeep();
        IReadOnlyList<IChildNodeType> GetDirectChildren(bool setInheritedFrom);
        IReadOnlyList<IChildNodeType> GetAllChildren();
        IReadOnlyList<INodeTypeLinked> GetDirectAncestors();
        IReadOnlyList<INodeTypeLinked> GetAllAncestors();
        IReadOnlyList<INodeTypeLinked> GetDirectDescendants();
        IReadOnlyList<INodeTypeLinked> GetAllDescendants();
        IReadOnlyList<INodeTypeLinked> GetNodeTypesThatEmbedded();
        IEnumerable<INodeTypeLinked> GetDirectProperties();
        IEnumerable<INodeTypeLinked> GetAllProperties();
        bool IsIdentical(INodeTypeLinked nodeType);
        string GetStringCode();
        Dictionary<string, string> GetPropertiesDict();
        IReadOnlyList<ISchemaCompareDiffInfo> GetDifference(INodeTypeLinked nodeType);
        List<NodeAggregationInfo> GetAttributeDotNamesRecursiveWithLimit(
            string path = null,
            string parent = null,
            int recursionLevel = 0);
        bool IsInheritedFrom(string nodeTypeName);
        bool IsObjectOfStudy { get; }
        bool IsEvent { get; }
        bool IsObjectSign { get; }
        bool IsEnum { get; }
        bool IsSeparateObject { get; }
        INodeTypeLinked GetNodeTypeByDotNameParts(string[] dotNameParts);
        IRelationTypeLinked GetRelationTypeByName(string name);
        IOntologySchema Schema { get; }
        IReadOnlyList<IRelationTypeLinked> GetComputedRelationTypes();
        string Formula { get; }
        bool IsComputed { get; }
    }

    public class NodeAggregationInfo
    {
        public string Name { get; set; }
        public bool IsAggregated { get; set; }
    }
}
