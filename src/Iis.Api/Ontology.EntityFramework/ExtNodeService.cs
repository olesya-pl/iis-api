using Iis.DataModel;
using Iis.Domain.ExtendedData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ExtNodeService: IExtNodeService
    {
        private readonly OntologyContext _context;

        public ExtNodeService(OntologyContext context)
        {
            _context = context;
        }

        public async Task<ExtNode> MapExtNodeAsync(NodeEntity nodeEntity, CancellationToken cancellationToken = default)
        {
            var extNode = new ExtNode
            {
                Id = nodeEntity.Id.ToString("N"),
                NodeTypeId = nodeEntity.NodeTypeId.ToString("N"),
                NodeTypeName = nodeEntity.NodeType?.Name,
                AttributeValue = nodeEntity.Attribute?.Value,
                CreatedAt = nodeEntity.CreatedAt,
                UpdatedAt = nodeEntity.UpdatedAt,
                Children = await GetExtNodesByIdsAsync(nodeEntity.OutgoingRelations.Select(r => r.TargetNodeId), cancellationToken)
            };
            return await Task.FromResult(extNode);
        }
        
        public async Task<ExtNode> GetExtNodeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var nodeEntity = await GetNodeQuery()
                .Where(n => n.Id == id)
                .SingleOrDefaultAsync();
            return await MapExtNodeAsync(nodeEntity, cancellationToken);
        }

        public async Task<List<ExtNode>> GetExtNodesByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var nodes = await GetNodeQuery()
                .Where(node => ids.Contains(node.Id))
                .ToListAsync();

            var result = await GetExtNodesAsync(nodes, cancellationToken);
            return result;
        }

        public async Task<List<ExtNode>> GetExtNodesByTypeIdsAsync(IEnumerable<Guid> nodeTypeIds, CancellationToken cancellationToken = default)
        {
            var nodes = await GetNodeQuery()
                .Where(node => nodeTypeIds.Contains(node.NodeTypeId))
                .ToListAsync();

            var result = await GetExtNodesAsync(nodes, cancellationToken);
            return result;
        }

        public async Task<List<Guid>> GetNodeTypesForElasticAsync(CancellationToken cancellationToken = default)
        {
            var typeNames = new List<string> { "Person", "Subdivision", "MilitaryMachinery", "Infrastructure", "Radionetwork" };
            return await _context.NodeTypes.Where(nt => typeNames.Contains(nt.Name)).Select(nt => nt.Id).ToListAsync();
        }

        private IQueryable<NodeEntity> GetNodeQuery()
        {
            return _context.Nodes
                .Include(n => n.Attribute)
                .Include(n => n.NodeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.Node)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.TargetNode);
        }

        private async Task<List<ExtNode>> GetExtNodesAsync(IEnumerable<NodeEntity> nodeEntities, CancellationToken cancellationToken = default)
        {
            var result = new List<ExtNode>();
            foreach (var node in nodeEntities)
            {
                var extNode = await MapExtNodeAsync(node, cancellationToken);
                result.Add(extNode);
            }
            return result;
        }
    }
}
