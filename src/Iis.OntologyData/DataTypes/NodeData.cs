﻿using Flee.PublicTypes;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public IRelation Relation { get; internal set; }
        public IAttribute Attribute { get; internal set; }
        public string Value => Attribute?.Value;

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
            return AllData.Locker.ReadLock(() => OutgoingRelations.Any(r => r.TypeName == propertyName && r.TargetNode.Value == value));
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
            return AllData.Locker.ReadLock(() => OutgoingRelations
                .SingleOrDefault(r => r.Node.NodeType.Name == name)
                ?.TargetNode);
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
            var replaced = ReplaceVariables(formula);

            var context = new ExpressionContext();
            context.Imports.AddType(typeof(ComputedPropertyFunctions));
            var eDynamic = context.CompileDynamic(replaced);
            var result = eDynamic.Evaluate();
            return result.ToString();
        }
        private string ReplaceVariables(string formula)
        {
            var regex = new Regex("[^{]*{([^}]+)}");
            var matches = regex.Matches(formula);
            var result = formula;
            foreach (Match match in matches)
            {
                var dotName = match.Groups[1].ToString();
                var value = GetSingleProperty(dotName)?.Value;
                result = result.Replace("{" + dotName + "}", "\"" + value + "\"");
            }
            return result;
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
    }
}
