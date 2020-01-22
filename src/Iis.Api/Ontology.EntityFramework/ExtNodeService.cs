using Iis.DataModel;
using Iis.Domain.ExtendedData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.EntityFramework
{
    public class ExtNodeService
    {
        private readonly OntologyContext _context;

        public ExtNodeService(OntologyContext context)
        {
            _context = context;
        }

        public async Task<ExtNode> MapExtNode(NodeEntity nodeEntity)
        {
            var extNode = new ExtNode
            {
                Id = nodeEntity.Id,
                NodeTypeId = nodeEntity.NodeTypeId,
                NodeTypeName = nodeEntity.NodeType?.Name,
                AttributeValue = nodeEntity.Attribute?.Value,
                CreatedAt = nodeEntity.CreatedAt,
                UpdatedAt = nodeEntity.UpdatedAt,
                Children = await GetExtNodesByIds(nodeEntity.OutgoingRelations.Select(r => r.Id))
            };
            return await Task.FromResult(extNode);
        }
        
        public async Task<ExtNode> GetExtNodeById(Guid id)
        {
            var nodeEntity = await GetNodeQuery()
                .Where(n => n.Id == id)
                .SingleOrDefaultAsync();
            return await MapExtNode(nodeEntity);
        }

        public async Task<List<ExtNode>> GetExtNodesByIds(IEnumerable<Guid> ids)
        {
            var nodes = await GetNodeQuery()
                .Where(n => ids.Contains(n.Id))
                .ToListAsync();

            var result = nodes.Select(async n => await MapExtNode(n)).Select(t => t.Result).ToList();
            return result;
        }

        private IQueryable<NodeEntity> GetNodeQuery()
        {
            return _context.Nodes
                .Include(n => n.Attribute)
                .Include(n => n.NodeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.TargetNode);
        }
    }
}
