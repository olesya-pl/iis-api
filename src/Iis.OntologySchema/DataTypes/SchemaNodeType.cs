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

        public IReadOnlyList<IChildNodeType> GetChildren()
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
                    EmbeddingOptions = r.EmbeddingOptions
                }).ToList();
        }
    }
}
