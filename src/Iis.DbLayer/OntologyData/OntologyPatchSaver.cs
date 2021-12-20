using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;

namespace Iis.DbLayer.OntologyData
{
    public class OntologyPatchSaver : IOntologyPatchSaver
    {
        private readonly IMapper _mapper;
        private readonly OntologyContext _context;
        public OntologyPatchSaver(OntologyContext context)
        {
            _context = context;
            _mapper = GetMapper();
        }
        public async Task SavePatchAsync(IOntologyPatch patch)
        {
            ApplyPatch(patch);
            await _context.SaveChangesAsync();
            patch.Clear();
        }
        public void SavePatch(IOntologyPatch patch)
        {
            ApplyPatch(patch);
            _context.SaveChanges();
            patch.Clear();
        }
        private static IMapper GetMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<INodeBase, NodeEntity>();
                cfg.CreateMap<IRelationBase, RelationEntity>();
                cfg.CreateMap<IAttributeBase, AttributeEntity>();
            });

            return new Mapper(configuration);
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
    }
}
