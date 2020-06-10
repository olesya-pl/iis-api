using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Types.Relay;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace IIS.Core.NodeMaterialRelation
{
    public class NodeMaterialRelationService
    {
        private readonly OntologyContext _context;
        private const string UniqueConstraintViolationMessage = "Could not create relation since there is already relation between given node and material.";

        public NodeMaterialRelationService(OntologyContext context)
        {
            _context = context;
        }

        public async Task Create(NodeMaterialRelation relation, EntityTypeNames entityType)
        {
            ValidateUniquness(relation);

            _context.MaterialFeatures.Add(new MaterialFeatureEntity
            {
                NodeId = relation.NodeId,
                NodeType = entityType,
                MaterialInfo = new MaterialInfoEntity
                {
                    MaterialId = relation.MaterialId
                }
            });
            await _context.SaveChangesAsync();
        }

        private void ValidateUniquness(NodeMaterialRelation relation)
        {
            if (_context.MaterialFeatures.Any(p => p.NodeId == relation.NodeId
                            && p.MaterialInfo.MaterialId == relation.MaterialId))
            {
                throw new Exception(UniqueConstraintViolationMessage);
            }
        }

        public async Task Delete(NodeMaterialRelation relation)
        {
            var featureToRemove = await _context.MaterialFeatures
                .Include(p => p.MaterialInfo)
                .FirstOrDefaultAsync(p => p.NodeId == relation.NodeId && p.MaterialInfo.MaterialId == relation.MaterialId);
            _context.MaterialInfos.Remove(featureToRemove.MaterialInfo);
            _context.MaterialFeatures.Remove(featureToRemove);
            await _context.SaveChangesAsync();
        }
    }
}
