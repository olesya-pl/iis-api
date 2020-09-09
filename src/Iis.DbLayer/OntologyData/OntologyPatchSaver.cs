using AutoMapper;
using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iis.DbLayer.OntologyData
{
    public class OntologyPatchSaver : IOntologyPatchSaver
    {
        IMapper _mapper;
        OntologyContext _context;
        public OntologyPatchSaver(OntologyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        private void ApplyPatch(IOntologyPatch patch)
        {
            var nodes = patch.Create.Nodes.Select(n => _mapper.Map<NodeEntity>(n));
            _context.Nodes.AddRange(nodes);
            _context.Relations.AddRange(patch.Create.Relations.Select(r => _mapper.Map<RelationEntity>(r)));
            _context.Attributes.AddRange(patch.Create.Attributes.Select(n => _mapper.Map<AttributeEntity>(n)));

            var nodeIds = patch.Update.Nodes.Select(p => p.Id).ToList();
            var nodeEntities = _context.Nodes.Where(p => nodeIds.Contains(p.Id)).ToDictionary(p => p.Id);
            foreach (var node in patch.Update.Nodes)
            {
                var nodeEntity = nodeEntities[node.Id];
                _mapper.Map(node, nodeEntity);
            }
            _context.Nodes.UpdateRange(nodeEntities.Values);
        }
        public async Task SavePatchAsync(IOntologyPatch patch)
        {
            ApplyPatch(patch);
            await _context.SaveChangesAsync();
        }
        public void SavePatch(IOntologyPatch patch)
        {
            ApplyPatch(patch);
            _context.SaveChanges();
        }
    }
}
