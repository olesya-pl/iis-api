using System;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace IIS.Core.NodeMaterialRelation
{
    public class NodeMaterialRelationService
    {
        private readonly OntologyContext _context;
        private readonly IOntologyNodesData _ontologyData;
        IChangeHistoryService _changeHistoryService;

        private const string UniqueConstraintViolationMessage = "Could not create relation since there is already relation between given node and material.";
        private const string NodeIdPropertyName = "MaterialFeature.NodeId";

        public NodeMaterialRelationService(OntologyContext context, 
            IOntologyNodesData ontologyData,
            IChangeHistoryService changeHistoryService)
        {
            _context = context;
            _ontologyData = ontologyData;
            _changeHistoryService = changeHistoryService;
        }

        public async Task Create(NodeMaterialRelation relation, string userName = null)
        {
            var material = GetMaterial(relation.MaterialId);

            if(material == null) throw new InvalidOperationException($"There is no Material with ID:{relation.MaterialId}");

            ValidateUniquness(relation);

            _context.MaterialFeatures.Add(new MaterialFeatureEntity
            {
                NodeId = relation.NodeId,
                MaterialInfo = new MaterialInfoEntity
                {
                    MaterialId = relation.MaterialId
                }
            });
            await _context.SaveChangesAsync();
            
            var changeHistoryDto = new ChangeHistoryDto
            {
                Date = DateTime.UtcNow,
                NewValue = relation.NodeId.ToString("N"),
                OldValue = null,
                PropertyName = NodeIdPropertyName,
                RequestId = Guid.NewGuid(),
                TargetId = relation.MaterialId,
                UserName = userName
            };
            await _changeHistoryService.SaveMaterialChanges(new[] { changeHistoryDto }, material.Title);
        }

        private void ValidateUniquness(NodeMaterialRelation relation)
        {
            if (_context.MaterialFeatures.Any(p => p.NodeId == relation.NodeId
                            && p.MaterialInfo.MaterialId == relation.MaterialId))
            {
                throw new Exception(UniqueConstraintViolationMessage);
            }
        }

        private MaterialEntity GetMaterial(Guid materialId)
        {
            return _context.Materials.SingleOrDefault(e => e.Id == materialId);
        }

        public async Task Delete(NodeMaterialRelation relation, string userName = null)
        {
            var material = GetMaterial(relation.MaterialId);

            var featureToRemove = await _context.MaterialFeatures
                .Include(p => p.MaterialInfo)
                .FirstOrDefaultAsync(p => p.NodeId == relation.NodeId && p.MaterialInfo.MaterialId == relation.MaterialId);
            _context.MaterialInfos.Remove(featureToRemove.MaterialInfo);
            _context.MaterialFeatures.Remove(featureToRemove);
            await _context.SaveChangesAsync();

            var changeHistoryDto = new ChangeHistoryDto
            {
                Date = DateTime.UtcNow,
                NewValue = null,
                OldValue = relation.NodeId.ToString("N"),
                PropertyName = NodeIdPropertyName,
                RequestId = Guid.NewGuid(),
                TargetId = relation.MaterialId,
                UserName = userName
            };
            await _changeHistoryService.SaveMaterialChanges(new[] { changeHistoryDto }, material.Title);
        }
    }
}
