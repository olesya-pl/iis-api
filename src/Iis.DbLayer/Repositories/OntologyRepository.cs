using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.DbLayer.Repositories
{
    public class OntologyRepository : IOntologyRepository
    {
        private const string ObjectSignType = "ObjectSign";
        private readonly IOntologySchema _ontologySchema;
        private readonly OntologyContext _context;

        public OntologyRepository(IOntologySchema ontologySchema, OntologyContext context)
        {
            _context = context;
            _ontologySchema = ontologySchema;
        }
        
        public async Task<IEnumerable<Guid>> GetFeatureIdListRelatedToNodeIdAsync(Guid nodeId)
        {
            var type = _ontologySchema.GetEntityTypeByName(ObjectSignType);

            var typeIdList = new List<Guid>();

            if (type != null)
            {
                typeIdList = type.IncomingRelations
                                    .Select(p => p.SourceTypeId)
                                    .ToList();
            }

            var result = await _context.Nodes
                                .Join(_context.Relations, n => n.Id, r => r.TargetNodeId, (node, relation) => new { Node = node, Relation = relation })
                                .Where(e => (!typeIdList.Any() ? true : typeIdList.Contains(e.Node.NodeTypeId)) && e.Relation.SourceNodeId == nodeId)
                                .AsNoTracking()
                                .Select(e => e.Node.Id)
                                .ToArrayAsync();
            return result;
        }
    }
}