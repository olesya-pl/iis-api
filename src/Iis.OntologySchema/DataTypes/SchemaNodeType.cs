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
        
        public IAttributeType AttributeType { get; set; }
        private SchemaRelationType _relationType;
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

        internal SchemaRelationType GetRelationByName(string relationName)
        {
            return _outgoingRelations
                .Where(r => r.NodeType.Name == relationName)
                .SingleOrDefault();
        }
    }
}
