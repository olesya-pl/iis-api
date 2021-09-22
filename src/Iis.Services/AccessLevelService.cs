using Iis.DataModel;
using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Common;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services
{
    public class AccessLevelService : IAccessLevelService
    {
        OntologyContext _context;
        ICommonData _commonData;
        IMaterialElasticService _materialElasticService;
        IOntologyNodesData _ontologyData;

        public AccessLevelService(
            OntologyContext context,
            ICommonData commonData,
            IMaterialElasticService materialElasticService,
            IOntologyNodesData ontologyData)
        {
            _context = context;
            _commonData = commonData;
            _materialElasticService = materialElasticService;
            _ontologyData = ontologyData;
        }

        public async Task ChangeAccessLevels(IAccessLevels newAccessLevels, Dictionary<Guid, Guid> mappings, CancellationToken ct)
        {
            var numericIndexMapping = GetNumericIndexMapping(newAccessLevels, mappings);
            var materialIds = ChangeAccessLevelsMaterials(numericIndexMapping);
            ChangeAccessLevelsOntology(mappings);
            _ontologyData.SaveAccessLevels(newAccessLevels);
            ChangeAccessLevelsUsers(numericIndexMapping);
            ChangeAccessLevelsReports(numericIndexMapping);
            await _context.SaveChangesAsync(ct);
            if (materialIds.Any())
                await _materialElasticService.PutCreatedMaterialsToElasticSearchAsync(materialIds, false, ct);
        }

        private List<Guid> ChangeAccessLevelsMaterials(Dictionary<int, int> mappings)
        {
            var result = new List<Guid>();
            var materials = _context.Materials.Where(m => mappings.Keys.Contains(m.AccessLevel));
            foreach (var material in materials)
            {
                material.AccessLevel = mappings[material.AccessLevel];
                result.Add(material.Id);
            }

            return result;
        }

        private void ChangeAccessLevelsUsers(Dictionary<int, int> mappings)
        {
            var users = _context.Users.Where(m => mappings.Keys.Contains(m.AccessLevel));
            foreach (var user in users)
            {
                user.AccessLevel = mappings[user.AccessLevel];
            }
        }

        private void ChangeAccessLevelsReports(Dictionary<int, int> mappings)
        {
            var reports = _context.Reports.Where(m => mappings.Keys.Contains(m.AccessLevel));
            foreach (var report in reports)
            {
                report.AccessLevel = mappings[report.AccessLevel];
            }
        }

        private void ChangeAccessLevelsOntology(Dictionary<Guid, Guid> mappings)
        {
            var objectTypeIds = _ontologyData.Schema
                .GetEntityTypes()
                .Where(nt => nt.IsObject)
                .Select(nt => nt.Id)
                .ToList();

            var nodes = _ontologyData.GetNodesByTypeIds(objectTypeIds);
            foreach (var node in nodes)
            {
                var accessLevelRelation = node.GetAccessLevelRelationId();
                if (accessLevelRelation != null && mappings.ContainsKey(accessLevelRelation.TargetNodeId))
                {
                    _ontologyData.UpdateRelationTarget(accessLevelRelation.Id, mappings[accessLevelRelation.TargetNodeId]);
                }
            }
        }

        private Dictionary<int, int> GetNumericIndexMapping(IAccessLevels newAccessLevels, Dictionary<Guid, Guid> deletedMappings)
        {
            var dict = new Dictionary<int, int>();
            
            foreach (var deletedId in deletedMappings.Keys)
            {
                var oldIndex = _commonData.AccessLevels.GetItemById(deletedId).NumericIndex;
                var newIndex = newAccessLevels.GetItemById(deletedMappings[deletedId]).NumericIndex;
                if (oldIndex != newIndex)
                {
                    dict[oldIndex] = newIndex;
                }
            }

            foreach (var oldItem in _commonData.AccessLevels.Items.Where(x => !deletedMappings.ContainsKey(x.Id)))
            {
                var newItem = newAccessLevels.GetItemById(oldItem.Id);
                if (oldItem.NumericIndex != newItem.NumericIndex)
                {
                    dict[oldItem.NumericIndex] = newItem.NumericIndex;
                }
            }
            
            return dict;
        }
    }
}