using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.SecurityLevels;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Controllers
{
    [Route("securityLevel")]
    [ApiController]
    public class SecurityLevelController : ControllerBase
    {
        private readonly IOntologyNodesData _ontologyData;
        private readonly ISecurityLevelChecker _securityLevelChecker;
        private readonly IUserService _userService;
        public SecurityLevelController(
            IOntologyNodesData ontologyData,
            ISecurityLevelChecker securityLevelChecker,
            IUserService userService)
        {
            _ontologyData = ontologyData;
            _securityLevelChecker = securityLevelChecker;
            _userService = userService;
        }

        [HttpGet("getSecurityLevels")]
        public IReadOnlyList<SecurityLevelPlain> GetSecurityLevels() => _securityLevelChecker.GetSecurityLevelsPlain();

        [HttpGet("getUserSecurityDtos")]
        public async Task<IReadOnlyList<UserSecurityDto>> GetUserSecurityDtos()
            => await _userService.GetUserSecurityDtosAsync();

        [HttpPost("saveUserSecurityDto")]
        public async Task SaveUserSecurityDto(UserSecurityDto userSecurityDto)
            => await _userService.SaveUserSecurityAsync(userSecurityDto);

        [HttpGet("getObjectSecurityDtos/{id}")]
        public async Task<ObjectSecurityDto> GetObjectSecurityDtos(Guid id)
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

        [HttpPost("saveObjectSecurityDto")]
        public async Task SaveObjectSecurityDto(ObjectSecurityDto objectSecurityDto)
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
