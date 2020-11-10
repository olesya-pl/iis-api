using Flee.PublicTypes;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Iis.OntologyData.DataTypes
{
    public class NodeData : INode
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        internal OntologyNodesData AllData { get; set; }

        public Guid NodeTypeId { get; set; }
        public INodeTypeLinked NodeType { get; internal set; }

        internal List<RelationData> _incomingRelations = new List<RelationData>();
        public IReadOnlyList<IRelation> IncomingRelations => _incomingRelations;

        internal List<RelationData> _outgoingRelations = new List<RelationData>();
        public IReadOnlyList<IRelation> OutgoingRelations => _outgoingRelations;

        internal IRelation _relation { get; set; }
        public IRelation Relation => _relation;
        public IAttribute Attribute { get; internal set; }
        public string Value => Attribute?.Value;

        public IReadOnlyList<IRelation> GetDirectRelations() =>
            AllData.Locker.ReadLock(() => _outgoingRelations.ToList());

        public override string ToString() => $"{NodeType.Name} {Id}";
        public IReadOnlyList<IRelation> GetInversedRelations()
        {
            return AllData.Locker.ReadLock(() =>
            {
                var result = new List<IRelation>();
                foreach (var relation in _incomingRelations.Where(r => r.Node.NodeType.HasInversed))
                {
                    //TODO: че-нибудь придумать чтобы инвертированные связи не создавались каждый раз как здрасьте
                    var relationType = relation.Node.NodeType.RelationType;
                    var inversedRelation = new RelationData
                    {
                        Id = relation.Id,
                        TargetNodeId = relation.SourceNodeId,
                        _targetNode = relation._sourceNode,
                        SourceNodeId = relation.TargetNodeId,
                        _sourceNode = relation._targetNode
                    };
                    inversedRelation._node = new NodeData
                    {
                        Id = relation.Id,
                        NodeTypeId = relationType.InversedRelationType.Id,
                        NodeType = relationType.InversedRelationType.NodeType,
                        _relation = inversedRelation,
                        AllData = AllData,
                        CreatedAt = relation.Node.CreatedAt,
                        UpdatedAt = relation.Node.UpdatedAt
                    };
                    result.Add(inversedRelation);
                }
                return result;
            });
        }
        public IDotNameValues GetDotNameValues()
        {
            return AllData.Locker.ReadLock(() =>
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
            });
        }
        public bool HasPropertyWithValue(string propertyName, string value)
        {
            return AllData.Locker.ReadLock(() => _outgoingRelations.Any(r => r.TypeName == propertyName && r.TargetNode.Value == value));
        }
        public INode GetChildNode(string childTypeName)
        {
            return AllData.Locker.ReadLock(() => _outgoingRelations
                .Where(r => r.Node.NodeType.Name == childTypeName)
                .Select(r => r.TargetNode)
                .SingleOrDefault());
        }
        public IReadOnlyList<INode> GetChildNodes(string childTypeName)
        {
            return AllData.Locker.ReadLock(() => _outgoingRelations
                .Where(r => r.Node.NodeType.Name == childTypeName)
                .Select(r => r.TargetNode)
                .ToList());
        }
        public INode GetSingleDirectProperty(string name)
        {
            return AllData.Locker.ReadLock(() => 
            {
                return _outgoingRelations
                    .SingleOrDefault(r => r.Node.NodeType.Name == name)
                    ?.TargetNode;
            });
        }
        public INode GetSingleProperty(IDotName dotName)
        {
            return AllData.Locker.ReadLock(() =>
            {
                INode currentNode = this;
                foreach (var name in dotName.Parts)
                {
                    currentNode = currentNode.GetSingleDirectProperty(name);
                    if (currentNode == null) return null;
                }
                return currentNode;
            });
        }
        public INode GetSingleProperty(string dotName)
        {
            return GetSingleProperty(NodeType.Schema.GetDotName(dotName));
        }
        public bool HasTheSameValues(INode another, IEnumerable<string> dotNames)
        {
            return AllData.Locker.ReadLock(() =>
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
            });
        }
        public bool AllValuesAreEmpty(IEnumerable<string> dotNames)
        {
            return AllData.Locker.ReadLock(() =>
            {
                foreach (var dotName in dotNames)
                {
                    if (!string.IsNullOrEmpty(GetSingleProperty(dotName)?.Value))
                    {
                        return false;
                    }
                }
                return true;
            });
        }
        public string ResolveFormula(string formula)
        {
            var singleFormulas = formula.Split(';').Select(s => s.Trim());
            string value = null;
            foreach (var singleFormula in singleFormulas)
            {
                value = ResolveSingleFormula(singleFormula)?.ToString();
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }
            return value;
        }
        private object ResolveSingleFormula(string formula)
        {
            var replaced = ReplaceVariables(formula);

            var context = new ExpressionContext();
            context.Imports.AddType(typeof(ComputedPropertyFunctions));
            context.Options.ParseCulture = CultureInfo.InvariantCulture;
            var eDynamic = context.CompileDynamic(replaced);
            var result = eDynamic.Evaluate();
            return result;
        }
        private string ReplaceVariables(string formula)
        {
            var regex = new Regex("[^{]*{([^}]+)}");
            var matches = regex.Matches(formula);
            var result = formula;
            foreach (Match match in matches)
            {
                var dotName = match.Groups[1].ToString();
                var value = GetSingleProperty(dotName)?.Value?.Replace("\"", "\\\"");
                result = result.Replace("{" + dotName + "}", "\"" + value + "\"");
            }
            return result;
        }
        public string GetComputedValue(string name)
        {
            var computedRelationType = NodeType.GetComputedRelationTypes().Where(rt => rt.NodeType.Name == name).SingleOrDefault();
            return computedRelationType == null ? null
                : ResolveFormula(computedRelationType.NodeType.MetaObject.Formula);
        }
        public IDotNameValues GetComputedValues()
        {
            var list = new List<DotNameValue>();
            foreach (var computedRelationType in NodeType.GetComputedRelationTypes())
            {
                list.Add(new DotNameValue
                {
                    DotName = computedRelationType.NodeType.Name,
                    Value = ResolveFormula(computedRelationType.NodeType.MetaObject.Formula)
                });
            }
            return new DotNameValues(list);
        }
        public IReadOnlyList<INode> GetDirectAttributeNodes(ScalarType? scalarType = null)
        {
            return _outgoingRelations
                .Where(r => r.IsLinkToAttribute
                    && (scalarType == null || r.TargetNode.NodeType.AttributeType.ScalarType == scalarType))
                .Select(r => r.TargetNode)
                .ToList();
        }
        public IReadOnlyList<INode> GetAllAttributeNodes(ScalarType? scalarType = null)
        {
            var result = new List<INode>();

            result.AddRange(GetDirectAttributeNodes(scalarType));
            foreach (var relation in _outgoingRelations.Where(r => r.IsLinkToInternalObject))
            {
                result.AddRange(relation.TargetNode.GetDirectAttributeNodes(scalarType));
            }

            return result;
        }
        public IReadOnlyList<IRelation> GetIncomingRelations(IEnumerable<string> relationTypeNameList)
        {
            return AllData.Locker.ReadLock(() => _incomingRelations
                .Where(r => relationTypeNameList == null ||
                    relationTypeNameList.Contains(r.Node.NodeType.Name))
                .ToList()
            );
        }
    }
}
