using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaNodeType: INodeType, INodeTypeLinked
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Meta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public Kind Kind { get; set; }
        public bool IsAbstract { get; set; }
        private List<IRelationTypeLinked> _incomingRelations = new List<IRelationTypeLinked>();
        public IReadOnlyList<IRelationTypeLinked> IncomingRelations => _incomingRelations;
        private List<IRelationTypeLinked> _outgoingRelations = new List<IRelationTypeLinked>();
        public IReadOnlyList<IRelationTypeLinked> OutgoingRelations => _outgoingRelations;
        
        public IAttributeType AttributeType { get; set; }
        public IRelationTypeLinked RelationType { get; set; }
        public void AddIncomingRelation(IRelationTypeLinked relationType)
        {
            _incomingRelations.Add(relationType);
        }
        public void AddOutgoingRelation(IRelationTypeLinked relationType)
        {
            _outgoingRelations.Add(relationType);
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

        public override string ToString()
        {
            return Name;
        }

        public bool IsEqual(INodeType nodeType)
        {
            return Id == nodeType.Id
                && Name == nodeType.Name
                && Title == nodeType.Title
                && Meta == nodeType.Meta
                && IsArchived == nodeType.IsArchived
                && Kind == nodeType.Kind
                && IsAbstract == nodeType.IsAbstract;
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
    }
}
