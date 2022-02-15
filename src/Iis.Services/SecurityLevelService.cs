using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.SecurityLevels;
using Iis.OntologyData.DataTypes;
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
        private readonly INodeSaveService _nodeSaveService;

        public SecurityLevelService(
            IOntologyNodesData ontologyData,
            ISecurityLevelChecker securityLevelChecker,
            OntologyContext context,
            INodeSaveService nodeSaveService)
        {
            _ontologyData = ontologyData;
            _securityLevelChecker = securityLevelChecker;
            _context = context;
            _nodeSaveService = nodeSaveService;
        }

        private INodeTypeLinked SecurityLevelType => _ontologyData.Schema.GetEntityTypeByName(EntityTypeNames.SecurityLevel.ToString());
        private IRelationType NameType => SecurityLevelType.GetRelationByName(OntologyNames.NameField);
        private IRelationType DescriptionType => SecurityLevelType.GetRelationByName(OntologyNames.DescriptionField);
        private IRelationType UniqueIndexType => SecurityLevelType.GetRelationByName(OntologyNames.UniqueIndexField);
        private IRelationType ParentType => SecurityLevelType.GetRelationByName(OntologyNames.ParentField);

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
            await _nodeSaveService.PutNodeAsync(node.Id);
        }

        public void SaveSecurityLevel(SecurityLevelPlain levelPlain)
        {
            _ontologyData.WriteLock(() =>
            {
                var node = _ontologyData.GetNode(levelPlain.Id) ?? _ontologyData.CreateNode(SecurityLevelType.Id);

                var nameRelation = node.GetSingleDirectRelation(OntologyNames.NameField);
                var oldName = nameRelation?.TargetNode.Value;

                if (nameRelation != null && levelPlain.Name != oldName)
                {
                    _ontologyData.RemoveNodeAndRelations(nameRelation.Id);
                }

                if (levelPlain.Name != oldName)
                {
                    _ontologyData.CreateRelationWithAttribute(node.Id, NameType.Id, levelPlain.Name);
                }

                if (levelPlain.IsNew)
                {
                    _ontologyData.CreateRelationWithAttribute(node.Id, UniqueIndexType.Id, GetNextUniqueIndex().ToString());
                }

                if (node.GetSingleDirectRelation(OntologyNames.ParentField) == null && levelPlain.ParentUniqueIndex != null)
                {
                    var parentLevel = _securityLevelChecker.GetSecurityLevel((int)levelPlain.ParentUniqueIndex);
                    _ontologyData.CreateRelation(node.Id, parentLevel.Id, ParentType.Id);
                }

                var descriptionRelation = node.GetSingleDirectRelation(OntologyNames.DescriptionField);
                var oldDescription = descriptionRelation?.TargetNode.Value;

                if (descriptionRelation != null && levelPlain.Description != oldDescription)
                {
                    _ontologyData.RemoveNodeAndRelations(descriptionRelation.Id);
                }

                if (levelPlain.Description != oldDescription)
                {
                    _ontologyData.CreateRelationWithAttribute(node.Id, DescriptionType.Id, levelPlain.Description);
                }
            });
            _securityLevelChecker.Reload();
        }

        public void RemoveSecurityLevel(Guid id)
        {
            _ontologyData.WriteLock(() =>
            {
                _ontologyData.RemoveNodeAndRelations(id);
                _securityLevelChecker.Reload();
            });
        }

        private int GetNextUniqueIndex()
        {
            var values =
               (from n in _context.Nodes
                join a in _context.Attributes on n.Id equals a.Id
                where n.NodeTypeId == UniqueIndexType.TargetTypeId
                select a.Value).ToList();

            return values.Count == 0 ? 0 : values.Max(_ => int.Parse(_)) + 1;
        }
    }
}
