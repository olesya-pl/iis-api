using Iis.DataModel;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
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

        public async Task<List<Guid>> GetExtNodesByTypeIdsAsync(IEnumerable<string> typeNames, CancellationToken cancellationToken = default)
        {
            var typeIds = await _context.NodeTypes.Where(nt => typeNames.Contains(nt.Name)).Select(nt => nt.Id).ToListAsync();

            return await GetNodeQuery()
                .Where(node => typeIds.Contains(node.NodeTypeId))
                .Select(p => p.Id)
                .ToListAsync();
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
                .Where(n => !n.IsArchived);
        }
    }
}
