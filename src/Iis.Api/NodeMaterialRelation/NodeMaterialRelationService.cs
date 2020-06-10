using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Types.Relay;
using Iis.DataModel;
using Iis.DataModel.Materials;
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

        public async Task Create(NodeMaterialRelation relation)
        {
            ValidateUniquness(relation);

            var nodeType = await GetNodeType(relation.NodeId);

            _context.MaterialFeatures.Add(new MaterialFeatureEntity
            {
                NodeId = relation.NodeId,
                NodeType = nodeType,
                MaterialInfo = new MaterialInfoEntity
                {
                    MaterialId = relation.MaterialId
                }
            });
            await _context.SaveChangesAsync();
        }

        private async Task<NodeEntityType> GetNodeType(Guid nodeId)
        {
            var node = await _context.Nodes
                .Include(p => p.NodeType)
                .FirstOrDefaultAsync(p => p.Id == nodeId);

            if (node == null)
            {
                throw new ArgumentNullException($"Could not find node by id {nodeId} while creating node-material relation");
            }

            return node.NodeType.Name switch
            {
                "Event" => NodeEntityType.Event,
                "ObjectSign" => NodeEntityType.Feature,
                _ => NodeEntityType.Entity
            };

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
