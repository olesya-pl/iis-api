using Iis.Interfaces.Meta;
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
        internal List<SchemaRelationType> _outgoingRelations = new List<SchemaRelationType>();
        public IReadOnlyList<IRelationTypeLinked> OutgoingRelations => _outgoingRelations;

        internal SchemaAttributeType _attributeType;
        public IAttributeType AttributeType => _attributeType;
        internal SchemaRelationType _relationType;
        public IRelationTypeLinked RelationType => _relationType;
        public Type ClrType
        {
            get
            {
                if (AttributeType == null) return null;

                switch (AttributeType.ScalarType)
                {
                    case ScalarType.String:
                        return typeof(string);
                    case ScalarType.Int:
                        return typeof(int);
                    case ScalarType.Decimal:
                        return typeof(decimal);
                    case ScalarType.Boolean:
                        return typeof(bool);
                    case ScalarType.Date:
                        return typeof(DateTime);
                    case ScalarType.Geo:
                        return typeof(Dictionary<string, object>);
                    case ScalarType.File:
                        return typeof(Guid);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public bool HasUniqueValues => UniqueValueFieldName != null;
        public ISchemaMeta MetaObject => new SchemaMeta(GetMetaDeep());
        public string GetMetaDeep()
        {
            if (!string.IsNullOrEmpty(Meta)) return Meta;
            var parent = GetDirectAncestors().FirstOrDefault();
            return parent?.GetMetaDeep();
        }

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

        public IRelationTypeLinked GetRelationTypeByName(string name)
        {
            var result = OutgoingRelations.FirstOrDefault(r => r.NodeType.Name == name);
            if (result == null)
            {
                foreach (var ancestor in GetAllAncestors())
                {
                    result = ancestor.OutgoingRelations.FirstOrDefault(r => r.NodeType.Name == name);
                    if (result != null) break;
                }
            }
            return result;
        }

        public IReadOnlyList<INodeTypeLinked> GetDirectDescendants()
        {
            return IncomingRelations.Where(r => r.Kind == RelationKind.Inheritance).Select(r => r.SourceType).ToList();
        }

        public IReadOnlyList<INodeTypeLinked> GetAllDescendants()
        {
            var result = new List<INodeTypeLinked>();
            var directDescendants = GetDirectDescendants();
            foreach (var directDescendant in directDescendants)
            {
                result.Add(directDescendant);
                result.AddRange(directDescendant.GetAllDescendants());
            }

            return result;
        }
        public IEnumerable<INodeTypeLinked> GetDirectProperties()
        {
            return IsInversed ? 
                new List<INodeTypeLinked> { RelationType.DirectRelationType.NodeType } :
                OutgoingRelations.Where(r => r.Kind == RelationKind.Embedding).Select(rt => rt.NodeType);
        }
        public IEnumerable<INodeTypeLinked> GetAllProperties()
        {
            var result = GetDirectProperties().ToList();
            var ancestors = GetAllAncestors();
            foreach (var ancestor in ancestors)
            {
                foreach (var property in ancestor.GetDirectProperties())
                {
                    if (!result.Any(p => p.Name == property.Name))
                    {
                        result.Add(property);
                    }
                }
            }
            return result;
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

            return result.Distinct().ToList();
        }

        public bool IsInheritedFrom(string nodeTypeName)
        {
            var ancestors = GetAllAncestors();
            return ancestors.Any(nt => nt.Name == nodeTypeName);
        }

        public bool IsObjectOfStudy => Name == EntityTypeNames.ObjectOfStudy.ToString() || IsInheritedFrom(EntityTypeNames.ObjectOfStudy.ToString());
        public bool IsEvent => string.Equals(Name, EntityTypeNames.Event.ToString());
        public bool IsObjectSign => Name == EntityTypeNames.ObjectSign.ToString() || IsInheritedFrom(EntityTypeNames.ObjectSign.ToString());
        public bool IsEnum => Name == EntityTypeNames.Enum.ToString() || IsInheritedFrom(EntityTypeNames.Enum.ToString());
        public bool IsAncestorOfObjectOfStudy => IsAbstract && 
            GetAllDescendants().All(nt => nt.IsAbstract || nt.IsObjectOfStudy);
        public bool IsSeparateObject => IsObjectOfStudy || IsObjectSign || IsEnum || IsAncestorOfObjectOfStudy;

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
        public bool HasInversed => MetaObject.Inversed != null;
        public bool IsInversed { get; private set; }

        public void SetIsInversed()
        {
            IsInversed = true;
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
                && UniqueValueFieldName == nodeType.UniqueValueFieldName
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

        public List<(string dotName, SchemaNodeType nodeType)> GetNodeTypesRecursive(string parentName = null)
        {
            var result = new List<(string dotName, SchemaNodeType nodeType)>();

            var dotName = parentName ?? Name;
            result.Add((dotName, this));

            foreach (var relationType in GetEmbeddingRelationsIncludeInherited())
            {
                if (relationType.TargetType.IsObjectOfStudy || relationType.TargetType.Name == Name) continue;

                var relationTypeName = $"{dotName}.{relationType.NodeType.Name}";
                var relationAttributes = relationType._targetType.GetNodeTypesRecursive(relationTypeName);
                result.AddRange(relationAttributes);
            }
            return result;
        }

        private IEnumerable<SchemaRelationType> GetEmbeddingRelationsIncludeInherited()
        {
            var result = _outgoingRelations.Where(r => r.Kind == RelationKind.Embedding).ToList();
            foreach (SchemaNodeType ancestor in GetAllAncestors())
            {
                result.AddRange(ancestor.GetEmbeddingRelationsIncludeInherited());
            }
            return result;
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

        internal SchemaRelationType GetRelationByName(string relationName)
        {
            return _outgoingRelations
                .Where(r => r.NodeType.Name == relationName)
                .SingleOrDefault();
        }

        public INodeTypeLinked GetNodeTypeByDotNameParts(string[] dotNameParts)
        {
            var nodeType = OutgoingRelations
                .Where(r => r.Kind == RelationKind.Embedding
                    && r.NodeType.Name == dotNameParts[0])
                .Select(r => r.TargetType)
                .Single();

            return dotNameParts.Length == 1 ? 
                nodeType : 
                nodeType.GetNodeTypeByDotNameParts(dotNameParts.Skip(1).ToArray());
        }

        public override string ToString() => Name;
        
        internal void RemoveRelationType(Guid id)
        {
            var relation = _outgoingRelations.Single(r => r.Id == id);
            if (relation != null)
            {
                _outgoingRelations.Remove(relation);
            }
        }
    }
}
