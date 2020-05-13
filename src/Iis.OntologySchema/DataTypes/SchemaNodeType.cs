using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.Comparison;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaNodeType: SchemaNodeTypeRaw, INodeType, INodeTypeLinked
    {
        private List<SchemaRelationType> _incomingRelations = new List<SchemaRelationType>();
        public IReadOnlyList<IRelationTypeLinked> IncomingRelations => _incomingRelations;
        private List<SchemaRelationType> _outgoingRelations = new List<SchemaRelationType>();
        public IReadOnlyList<IRelationTypeLinked> OutgoingRelations => _outgoingRelations;

        internal SchemaAttributeType _attributeType;
        public IAttributeType AttributeType => _attributeType;
        internal SchemaRelationType _relationType;
        public IRelationTypeLinked RelationType => _relationType;
        internal void AddIncomingRelation(SchemaRelationType relationType)
        {
            _incomingRelations.Add(relationType);
        }
        internal void AddOutgoingRelation(SchemaRelationType relationType)
        {
            _outgoingRelations.Add(relationType);
        }

        internal void SetRelationType(SchemaRelationType relationType)
        {
            _relationType = relationType;
        }

        public IReadOnlyList<IChildNodeType> GetDirectChildren(bool setInheritedFrom)
        {
            return OutgoingRelations
                .Where(r => r.Kind == RelationKind.Embedding)
                .Select(r => new SchemaChildNodeType
                {
                    Id = r.TargetType.Id,
                    Name = r.TargetType.Name,
                    Title = r.TargetType.Title,
                    Kind = r.TargetType.Kind,
                    RelationId = r.NodeType.Id,
                    RelationName = r.NodeType.Name,
                    RelationTitle = r.NodeType.Title,
                    RelationMeta = r.NodeType.Meta,
                    ScalarType = r.TargetType.AttributeType?.ScalarType,
                    EmbeddingOptions = r.EmbeddingOptions,
                    InheritedFrom = setInheritedFrom ? this.Name : string.Empty,
                    TargetType = r.TargetType
                }).ToList();
        }

        public IReadOnlyList<IChildNodeType> GetAllChildren()
        {
            var result = new List<IChildNodeType>();
            result.AddRange(GetDirectChildren(false));
            var ancestors = GetAllAncestors();
            foreach (var ancestor in ancestors)
            {
                result.AddRange(ancestor.GetDirectChildren(true));
            }

            return result;
        }

        public IReadOnlyList<INodeTypeLinked> GetDirectDescendants()
        {
            return IncomingRelations.Where(r => r.Kind == RelationKind.Inheritance).Select(r => r.SourceType).ToList();
        }

        public IReadOnlyList<INodeTypeLinked> GetNodeTypesThatEmbedded()
        {
            return IncomingRelations.Where(r => r.Kind == RelationKind.Embedding).Select(r => r.SourceType).ToList();
        }

        public IReadOnlyList<INodeTypeLinked> GetDirectAncestors()
        {
            return OutgoingRelations.Where(r => r.Kind == RelationKind.Inheritance).Select(r => r.TargetType).ToList();
        }

        public IReadOnlyList<INodeTypeLinked> GetAllAncestors()
        {
            var result = new List<INodeTypeLinked>();
            var directAncestors = GetDirectAncestors();
            foreach (var directAncestor in directAncestors)
            {
                result.Add(directAncestor);
                result.AddRange(directAncestor.GetAllAncestors());
            }

            return result;
        }

        public string GetStringCode()
        {
            switch (Kind)
            {
                case Kind.Entity:
                    return Name;
                case Kind.Attribute:
                    return null;
                case Kind.Relation:
                    if (RelationType.Kind == RelationKind.Inheritance)
                    {
                        return $"{RelationType.SourceType.Name}=>{RelationType.TargetType.Name}";
                    }
                    else if (RelationType.TargetType.Kind == Kind.Entity)
                    {
                        return $"{RelationType.SourceType.Name}->{RelationType.NodeType.Name}";
                    }
                    else
                    {
                        return $"{RelationType.SourceType.Name}.{RelationType.NodeType.Name}";
                    }
            }

            return null;
        }

        public bool IsIdentical(INodeTypeLinked nodeType)
        {
            if (!IsIdenticalBase(nodeType)) return false;

            if (_relationType == null) return true;

            if (_relationType.Kind == RelationKind.Inheritance || _relationType.TargetType.Kind == Kind.Entity)
            {
                return RelationType.IsIdentical(nodeType.RelationType, false);
            }

            if (_relationType.Kind == RelationKind.Embedding && _relationType.TargetType.Kind == Kind.Attribute)
            {
                return RelationType.IsIdentical(nodeType.RelationType, true);
            }

            throw new ArgumentException($"IsIdentical met sad situation with item {GetStringCode()}");
        }

        private bool IsIdenticalBase(INodeTypeLinked nodeType)
        {
            var scalarTypesAreEqual = Kind == Kind.Attribute ?
                AttributeType.ScalarType == nodeType.AttributeType.ScalarType :
                true;

            return Name == nodeType.Name
                && Title == nodeType.Title
                && Meta == nodeType.Meta
                && IsArchived == nodeType.IsArchived
                && Kind == nodeType.Kind
                && IsAbstract == nodeType.IsAbstract
                && scalarTypesAreEqual;
        }

        public Dictionary<string, string> GetPropertiesDict()
        {
            var dict = new Dictionary<string, string>();
            dict[nameof(Name)] = Name;
            dict[nameof(Title)] = Title;
            dict[nameof(Meta)] = Meta;
            dict[nameof(IsArchived)] = IsArchived.ToString();
            dict[nameof(Kind)] = Kind.ToString();
            dict[nameof(IsAbstract)] = IsAbstract.ToString();
            dict["ScalarType"] = AttributeType?.ScalarType.ToString() ?? string.Empty;
            dict["EmbeddingOptions"] = RelationType?.EmbeddingOptions.ToString() ?? string.Empty;
            dict["RelationKind"] = RelationType?.Kind.ToString() ?? string.Empty;
            dict["RelationSourceName"] = RelationType?.SourceType.Name ?? string.Empty;
            dict["RelationTargetName"] = RelationType?.TargetType.Name ?? string.Empty;
            return dict;
        }

        public IReadOnlyList<ISchemaCompareDiffInfo> GetDifference(INodeTypeLinked nodeType)
        {
            var result = new List<ISchemaCompareDiffInfo>();
            var thisDict = GetPropertiesDict();
            var anotherDict = nodeType.GetPropertiesDict();
            foreach (var key in thisDict.Keys)
            {
                if (thisDict[key] != anotherDict[key])
                {
                    result.Add(new SchemaCompareDiffInfo
                    {
                        PropertyName = key,
                        OldValue = anotherDict[key],
                        NewValue = thisDict[key]
                    });
                }
            }
            return result;
        }

        public void CopyFrom(INodeType nodeType)
        {
            Name = nodeType.Name;
            Title = nodeType.Title;
            Meta = nodeType.Meta;
            IsArchived = nodeType.IsArchived;
            Kind = nodeType.Kind;
            IsAbstract = nodeType.IsAbstract;
        }

        public List<string> GetAttributeDotNamesRecursive(string parentName = null)
        {
            var result = new List<string>();

            if (Kind == Kind.Attribute)
            {
                result.Add(Name);
            }

            foreach (var relation in OutgoingRelations)
            {
                string relationName = relation.Kind == RelationKind.Embedding && relation.TargetType.Kind == Kind.Entity ? relation.NodeType.Name : null;
                result.AddRange(relation.TargetType.GetAttributeDotNamesRecursive(relationName));
            }

            return result.Select(name => (parentName == null ? name : $"{parentName}.{name}")).ToList();
        }

        public List<string> GetAttributeDotNamesRecursiveWithLimit(string parentName = null, int recursionLevel = 0)
        {
            const int MaxRecursionLevel = 4;
            var result = new List<string>();

            if (Kind == Kind.Attribute)
            {
                result.Add(Name);
            }
            var isTopLevel = recursionLevel == 0;
            if (isTopLevel)
            {
                result.AddRange(new[] { "NodeTypeName", "NodeTypeTitle", "CreatedAt", "UpdatedAt" });
            }

            if (recursionLevel == MaxRecursionLevel)
            {
                return result;
            }
            foreach (var relation in OutgoingRelations)
            {
                string relationName = relation.Kind == RelationKind.Embedding && relation.TargetType.Kind == Kind.Entity
                    ? relation.NodeType.Name
                    : null;
                result.AddRange(relation.TargetType.GetAttributeDotNamesRecursiveWithLimit(relationName, recursionLevel + 1));
            }

            return result.Select(name => (parentName == null ? name : $"{parentName}.{name}")).ToList();
        }

        private List<string> GetDotNameToRoot(Guid rootTypeId)
        {
            foreach (var relation in _incomingRelations)
            {
                if (relation._sourceType.Id == rootTypeId)
                {
                    return new List<string> { relation._nodeType.Name };
                }
                var list = relation._sourceType.GetDotNameToRoot(rootTypeId);
                if (list.Count > 0)
                {
                    list.Add(relation._nodeType.Name);
                    return list;
                }
            }
            return new List<string>();
        }

        public string GetAttributeTypeDotName(Guid rootTypeId)
        {
            var list = GetDotNameToRoot(rootTypeId);
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.Append(item + ".");
            }
            sb.Remove(sb.Length-1, 1);
            return sb.ToString();
        }

        internal SchemaRelationType GetRelationByName(string relationName)
        {
            return _outgoingRelations
                .Where(r => r.NodeType.Name == relationName)
                .SingleOrDefault();
        }

        public override string ToString() => Name;
    }
}
