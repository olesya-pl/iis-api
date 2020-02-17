using Iis.DataModel;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Ontology;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class ExtNodeService : IExtNodeService
    {
        private readonly OntologyContext _context;

        public ExtNodeService(OntologyContext context)
        {
            _context = context;
        }

        public async Task<IExtNode> MapExtNodeAsync(NodeEntity nodeEntity, string nodeTypeName, CancellationToken cancellationToken = default)
        {
            var extNode = new ExtNode
            {
                Id = nodeEntity.Id.ToString("N"),
                NodeTypeId = nodeEntity.NodeTypeId.ToString("N"),
                NodeTypeName = nodeTypeName,
                AttributeValue = nodeEntity.Attribute?.Value,
                CreatedAt = nodeEntity.CreatedAt,
                UpdatedAt = nodeEntity.UpdatedAt,
                Children = await GetExtNodesByRelations(nodeEntity.OutgoingRelations, cancellationToken)
            };
            return await Task.FromResult(extNode);
        }

        public async Task<IExtNode> GetExtNodeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var nodeEntity = await GetNodeQuery()
                .Where(n => n.Id == id)
                .SingleOrDefaultAsync();
            return await MapExtNodeAsync(nodeEntity, nodeEntity.NodeType.Name, cancellationToken);
        }

        public async Task<List<IExtNode>> GetExtNodesByRelations(IEnumerable<RelationEntity> relations, CancellationToken cancellationToken = default)
        {
            var result = new List<IExtNode>();
            foreach (var relation in relations)
            {
                var node = await GetNodeQuery().Where(node => node.Id == relation.TargetNodeId).SingleOrDefaultAsync();
                var extNode = await MapExtNodeAsync(node, relation.Node.NodeType.Name, cancellationToken);
                result.Add(extNode);
            }
            return result;
        }

        public async Task<List<IExtNode>> GetExtNodesByTypeIdsAsync(List<string> typeNames, CancellationToken cancellationToken = default)
        {
            var typeIds = await _context.NodeTypes.Where(nt => typeNames.Contains(nt.Name)).Select(nt => nt.Id).ToListAsync();

            var nodes = await GetNodeQuery()
                .Where(node => typeIds.Contains(node.NodeTypeId))
                .ToListAsync();

            int cnt = 0, total = nodes.Count();
            var result = new List<IExtNode>();

            foreach (var node in nodes)
            {
                var extNode = await GetExtNodeByIdAsync(node.Id);
                Console.WriteLine($"{++cnt}/{total}: {extNode.NodeTypeName}; {node.Id}");
                result.Add(extNode);
            }

            return result;
        }

        private IQueryable<NodeEntity> GetNodeQuery()
        {
            return _context.Nodes
                .Include(n => n.Attribute)
                .Include(n => n.NodeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.Node)
                .ThenInclude(rn => rn.NodeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.TargetNode);
        }
    }
}
