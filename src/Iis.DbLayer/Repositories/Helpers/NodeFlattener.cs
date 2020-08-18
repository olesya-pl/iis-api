using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories.Helpers
{
    public class NodeFlattener
    {
        private readonly OntologyContext _context;
        private readonly IElasticSerializer _elasticSerializer;

        public NodeFlattener(OntologyContext context,
            IElasticSerializer elasticSerializer)
        {
            _context = context;
            _elasticSerializer = elasticSerializer;
        }

        private async Task<ExtNode> GetExtNodeByIdAsync(
            Guid id,
            List<Guid> visitedRelationIds,
            CancellationToken cancellationToken = default)
        {
            var nodeEntity = await GetNodeQuery()
                .Where(n => n.Id == id)
                .SingleOrDefaultAsync();

            if (nodeEntity == null) return null;

            var extNode = await MapExtNodeAsync(
                nodeEntity,
                nodeEntity.NodeType.Name,
                nodeEntity.NodeType.Title,
                visitedRelationIds,
                cancellationToken);
            return extNode;
        }

        private async Task<ExtNode> MapExtNodeAsync(
            NodeEntity nodeEntity,
            string nodeTypeName,
            string nodeTypeTitle,
            List<Guid> visitedRelationIds,
            CancellationToken cancellationToken = default)
        {
            var extNode = new ExtNode
            {
                Id = nodeEntity.Id.ToString("N"),
                NodeTypeId = nodeEntity.NodeTypeId.ToString("N"),
                NodeTypeName = nodeTypeName,
                NodeTypeTitle = nodeTypeTitle,
                EntityTypeName = nodeEntity.NodeType.Name,
                AttributeValue = GetAttributeValue(nodeEntity),
                ScalarType = nodeEntity.NodeType?.IAttributeTypeModel?.ScalarType,
                CreatedAt = nodeEntity.CreatedAt,
                UpdatedAt = nodeEntity.UpdatedAt,
                Children = await GetExtNodesByRelations(nodeEntity.OutgoingRelations, visitedRelationIds, cancellationToken)
            };
            return extNode;
        }

        private object GetAttributeValue(NodeEntity nodeEntity)
        {
            if (nodeEntity.Attribute == null) return null;

            var scalarType = nodeEntity.NodeType.IAttributeTypeModel.ScalarType;
            var value = nodeEntity.Attribute.Value;
            switch (scalarType)
            {
                case ScalarType.Int:
                    return Convert.ToInt32(value);
                case ScalarType.Date:
                //return Convert.ToDateTime(value);
                default:
                    return value.ToString();
            }
        }

        private async Task<List<ExtNode>> GetExtNodesByRelations(
            IEnumerable<RelationEntity> relations,
            List<Guid> visitedRelationIds,
            CancellationToken cancellationToken = default)
        {
            var result = new List<ExtNode>();
            foreach (var relation in relations.Where(r => !r.Node.IsArchived && !r.Node.NodeType.IsArchived
                && !r.TargetNode.IsArchived && !r.TargetNode.NodeType.IsArchived))
            {
                if (!visitedRelationIds.Contains(relation.Id))
                {
                    visitedRelationIds.Add(relation.Id);
                    var node = await GetNodeQuery().Where(node => node.Id == relation.TargetNodeId).SingleOrDefaultAsync();
                    var extNode = await MapExtNodeAsync(
                        node,
                        relation.Node.NodeType.Name,
                        relation.Node.NodeType.Title,
                        visitedRelationIds,
                        cancellationToken);
                    result.Add(extNode);
                }
            }
            return result;
        }

        private IQueryable<NodeEntity> GetNodeQuery()
        {
            return _context.Nodes
                .Include(n => n.Attribute)
                .Include(n => n.NodeType)
                .ThenInclude(nt => nt.IAttributeTypeModel)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.Node)
                .ThenInclude(rn => rn.NodeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.TargetNode)
                .ThenInclude(tn => tn.NodeType)
                .Where(n => !n.IsArchived);
        }

        public async Task<FlattenNodeResult> FlattenNode(Guid id, CancellationToken cancellationToken = default)
        {
            var extNode = await GetExtNodeByIdAsync(id, new List<Guid>(), cancellationToken);
            return new FlattenNodeResult
            {
                SerializedNode = _elasticSerializer.GetJsonByExtNode(extNode),
                Id = extNode.Id,
                NodeTypeName = extNode.NodeTypeName
            };
        }
    }

    public class FlattenNodeResult
    {
        public string SerializedNode { get; set; }
        public string Id { get; internal set; }
        public string NodeTypeName { get; internal set; }
    }
}
