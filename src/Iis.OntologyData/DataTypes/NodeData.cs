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
                    list.Add(new DotNameValue(
                        relation.TypeName, 
                        relation.TargetNode.Value,
                        new List<INode> { relation.Node, relation.TargetNode }));
                }
                else if (relation.IsLinkToSeparateObject)
                {
                    list.Add(new DotNameValue(
                        relation.TypeName, 
                        relation.TargetNodeId.ToString(),
                        new List<INode> { relation.Node }));
                }
                else
                {
                    var values = relation._targetNode.GetDotNameValues();
                    foreach (var item in values.Items)
                    {
                        list.Add(new DotNameValue(
                            $"{NodeType.Name}.{item.DotName}", 
                            item.Value, 
                            new List<INode> { relation.Node, relation.TargetNode }
                                .Concat(item.Nodes)));
                    }
                }
            }
            return new DotNameValues(list);
        }
        public INode GetSingleDirectProperty(string name)
        {
            return OutgoingRelations
                .SingleOrDefault(r => r.Node.NodeType.Name == name)
                ?.TargetNode;
        }
        public INode GetSingleProperty(IDotName dotName)
        {
            INode currentNode = this;
            foreach (var name in dotName.Parts)
            {
                currentNode = currentNode.GetSingleDirectProperty(name);
                if (currentNode == null) return null;
            }
            return currentNode;
        }
        public INode GetSingleProperty(string dotName)
        {
            return GetSingleProperty(NodeType.Schema.GetDotName(dotName));
        }
        public bool HasTheSameValues(INode another, IEnumerable<string> dotNames)
        {
            if (another == null) return false;
            foreach (var dotName in dotNames)
            {
                if (this.GetSingleProperty(dotName)?.Value != another.GetSingleProperty(dotName)?.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public bool AllValuesAreEmpty(IEnumerable<string> dotNames)
        {
            foreach (var dotName in dotNames)
            {
                if (!string.IsNullOrEmpty(GetSingleProperty(dotName)?.Value)) 
                {
                    return false;
                }
            }
            return true;
        }
    }
}
