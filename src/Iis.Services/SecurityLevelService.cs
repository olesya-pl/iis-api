using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.SecurityLevels;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Services
{
    public class SecurityLevelService : ISecurityLevelService
    {
        private readonly OntologyContext _context;
        private readonly IOntologyNodesData _ontologyData;
        private readonly ISecurityLevelChecker _securityLevelChecker;

        public SecurityLevelService(
            IOntologyNodesData ontologyData,
            ISecurityLevelChecker securityLevelChecker,
            OntologyContext context)
        {
            _ontologyData = ontologyData;
            _securityLevelChecker = securityLevelChecker;
        }

        public IReadOnlyList<SecurityLevelPlain> GetSecurityLevelsPlain()
            => _securityLevelChecker.GetSecurityLevelsPlain();

        public async Task<ObjectSecurityDto> GetObjectSecurityDtosAsync(Guid id)
        {
            var node = _ontologyData.GetNode(id);
            if (node == null) throw new Exception($"Node with id = {id} is not found");
            return new ObjectSecurityDto
            {
                Id = id,
                Title = node.GetTitleValue(),
                SecurityIndexes = node.GetSecurityLevelIndexes()
            };
        }

        public async Task SaveObjectSecurityDtoAsync(ObjectSecurityDto objectSecurityDto)
        {
            var node = _ontologyData.GetNode(objectSecurityDto.Id);
            if (node == null) throw new Exception($"Node with id = {objectSecurityDto.Id} is not found");
            var newLevels = _securityLevelChecker.GetSecurityLevels(objectSecurityDto.SecurityIndexes);
            var relations = node.GetSecurityLevelRelations();
            var idsToDelete = relations
                .Where(r => !newLevels.Any(l => l.Id == r.TargetNodeId))
                .Select(r => r.Id)
                .ToList();

            var idsToAdd = newLevels
                .Where(l => !relations.Any(r => r.TargetNodeId == l.Id))
                .Select(l => l.Id)
                .ToList();

            _ontologyData.WriteLock(() =>
            {
                _ontologyData.RemoveNodes(idsToDelete);
                var objectType = _ontologyData.Schema.GetEntityTypeByName(EntityTypeNames.Object.ToString());
                var securityLevelType = objectType.GetRelationByName(OntologyNames.SecurityLevelField);

                foreach (var id in idsToAdd)
                {
                    _ontologyData.CreateRelation(objectSecurityDto.Id, id, securityLevelType.Id);
                }
            });
        }
    }
}
