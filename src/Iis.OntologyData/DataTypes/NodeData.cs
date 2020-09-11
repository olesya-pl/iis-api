using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class NodeData : INode
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }

        public Guid NodeTypeId { get; set; }
        public INodeTypeLinked NodeType { get; internal set; }

        internal List<RelationData> _incomingRelations = new List<RelationData>();
        public IReadOnlyList<IRelation> IncomingRelations => _incomingRelations;

        internal List<RelationData> _outgoingRelations = new List<RelationData>();
        public IReadOnlyList<IRelation> OutgoingRelations => _outgoingRelations;

        public IRelation Relation { get; internal set; }

        public IAttribute Attribute { get; internal set; }

        public string Value => Attribute?.Value;

        public IDotNameValues GetDotNameValues()
        {
            var list = new List<DotNameValue>();
            foreach (var relation in _outgoingRelations)
            {
                if (relation.TargetKind == Kind.Attribute)
                {
                    list.Add(new DotNameValue(relation.TypeName, relation.TargetNode.Value));
                }
                else if (relation.IsLinkToSeparateObject)
                {
                    list.Add(new DotNameValue(relation.TypeName, relation.TargetNodeId.ToString()));
                }
                else
                {
                    var values = relation._targetNode.GetDotNameValues();
                    foreach (var item in values.Items)
                    {
                        list.Add(new DotNameValue(
                            $"{NodeType.Name}.{item.DotName}", item.Value));
                    }
                }
            }
            return new DotNameValues(list);
        }
        public INode GetChildNode(string childTypeName)
        {
            return _outgoingRelations
                .Where(r => r.Node.NodeType.Name == childTypeName)
                .Select(r => r.TargetNode)
                .SingleOrDefault();
        }
        public IReadOnlyList<INode> GetChildNodes(string childTypeName)
        {
            return _outgoingRelations
                .Where(r => r.Node.NodeType.Name == childTypeName)
                .Select(r => r.TargetNode)
                .ToList();
        }
    }
}
