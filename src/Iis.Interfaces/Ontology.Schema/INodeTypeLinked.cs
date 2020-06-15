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
        string Aliases { get; }
        IReadOnlyList<IRelationTypeLinked> IncomingRelations { get; }
        IReadOnlyList<IRelationTypeLinked> OutgoingRelations { get; }
        IAttributeType AttributeType { get; }
        IRelationTypeLinked RelationType { get; }
        IReadOnlyList<IChildNodeType> GetDirectChildren(bool setInheritedFrom);
        IReadOnlyList<IChildNodeType> GetAllChildren();
        IReadOnlyList<INodeTypeLinked> GetDirectAncestors();
        IReadOnlyList<INodeTypeLinked> GetAllAncestors();
        IReadOnlyList<INodeTypeLinked> GetDirectDescendants();
        IReadOnlyList<INodeTypeLinked> GetAllDescendants();
        IReadOnlyList<INodeTypeLinked> GetNodeTypesThatEmbedded();
        bool IsIdentical(INodeTypeLinked nodeType);
        string GetStringCode();
        Dictionary<string, string> GetPropertiesDict();
        IReadOnlyList<ISchemaCompareDiffInfo> GetDifference(INodeTypeLinked nodeType);
        List<string> GetAttributeDotNamesRecursive(string parentName = null);
        List<string> GetAttributeDotNamesRecursiveWithLimit(string parentName = null, int recursionLevel = 0);
        IAttributeInfoList GetAttributesInfo();
        bool IsInheritedFrom(string nodeTypeName);
        bool IsObjectOfStudy { get; }
        bool IsEvent { get; }
        INodeTypeLinked GetNodeTypeByDotNameParts(string[] dotNameParts);
    }
}
