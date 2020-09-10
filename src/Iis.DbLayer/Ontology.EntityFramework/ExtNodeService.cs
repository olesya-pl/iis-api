using Iis.DataModel;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using IIS.Repository;
using IIS.Repository.Factories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class ExtNodeService<TUnitOfWork> : BaseService<TUnitOfWork>, IExtNodeService where TUnitOfWork : IIISUnitOfWork
    {
        private const string Iso8601DateFormat = "yyyy-MM-dd'T'HH:mm:ssZ";
        private readonly OntologyContext _context;
        private readonly IOntologySchema _ontologySchema;

        public ExtNodeService(OntologyContext context,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IOntologySchema ontologySchema) : base(unitOfWorkFactory)
        {
            _context = context;
            _ontologySchema = ontologySchema;
        }

        public async Task<List<Guid>> GetExtNodesByTypeIdsAsync(IEnumerable<string> typeNames, CancellationToken cancellationToken = default)
        {
            //TODO: should be refactored using IUnitOfWork instead of OntologyContext
            var typeIds = await _context.NodeTypes.Where(nt => typeNames.Contains(nt.Name)).Select(nt => nt.Id).ToListAsync();

            return await GetNodeQuery()
                .Where(node => typeIds.Contains(node.NodeTypeId))
                .Select(p => p.Id)
                .ToListAsync();
        }

        public async Task<IExtNode> GetExtNodeAsync(Guid id, CancellationToken ct = default)
        {
            var nodeEntity = await RunWithoutCommitAsync(async uow => await uow.OntologyRepository.GetNodeEntityWithIncludesByIdAsync(id));

            if (nodeEntity == null) return null;

            var extNode = await MapExtNodeAsync(
                nodeEntity,
                nodeEntity.NodeType.Name,
                nodeEntity.NodeType.Title,
                new List<Guid> { id },
                ct);
            return extNode;
        }

        private async Task<ExtNode> MapExtNodeAsync(
            NodeEntity nodeEntity,
            string nodeTypeName,
            string nodeTypeTitle,
            List<Guid> visitedEntityIds,
            CancellationToken cancellationToken = default)
        {
            if (_ontologySchema.GetNodeTypeById(nodeEntity.NodeTypeId).IsObjectOfStudy) 
            {
                visitedEntityIds.Add(nodeEntity.Id);
            }
            var extNode = new ExtNode
            {
                Id = nodeEntity.Id.ToString("N"),
                NodeTypeId = nodeEntity.NodeTypeId.ToString("N"),
                NodeTypeName = nodeTypeName,
                NodeTypeTitle = nodeTypeTitle,
                EntityTypeName = nodeEntity.NodeType.Name,
                AttributeValue = GetAttributeValue(nodeEntity),
                ScalarType = nodeEntity.NodeType?.IAttributeTypeModel?.ScalarType,
                CreatedAt = nodeEntity.CreatedAt,
                UpdatedAt = nodeEntity.UpdatedAt,
                Children = await GetExtNodesByRelations(nodeEntity.OutgoingRelations, visitedEntityIds, cancellationToken)
            };
            return extNode;
        }

        private object GetAttributeValue(NodeEntity nodeEntity)
        {
            if (nodeEntity.Attribute == null) return null;

            var scalarType = nodeEntity.NodeType.IAttributeTypeModel.ScalarType;
            var value = nodeEntity.Attribute.Value;
            switch (scalarType)
            {
                case ScalarType.Int:
                    return Convert.ToInt32(value);
                case ScalarType.Date:
                    {
                        if (DateTime.TryParse(value, out DateTime dateTimeValue))
                        {
                            return dateTimeValue.ToString(Iso8601DateFormat);
                        }
                        else
                        {
                            return value;
                        }
                    }
                default:
                    return value;
            }
        }

        private async Task<List<ExtNode>> GetExtNodesByRelations(
            IEnumerable<RelationEntity> relations,
            List<Guid> visitedEntityIds,
            CancellationToken cancellationToken = default)
        {
            var result = new List<ExtNode>();
            foreach (var relation in relations.Where(r => !r.Node.IsArchived && !r.Node.NodeType.IsArchived
                && !r.TargetNode.IsArchived && !r.TargetNode.NodeType.IsArchived))
            {
                var node = await RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.OntologyRepository.GetNodeEntityWithIncludesByIdAsync(relation.TargetNodeId));
                if (!visitedEntityIds.Contains(node.Id))
                {
                    var extNode = await MapExtNodeAsync(
                        node,
                        relation.Node.NodeType.Name,
                        relation.Node.NodeType.Title,
                        visitedEntityIds,
                        cancellationToken);
                    result.Add(extNode);
                }
            }
            return result;
        }

        private IQueryable<NodeEntity> GetNodeQuery()
        {
            return _context.Nodes
                .Include(n => n.Attribute)
                .Include(n => n.NodeType)
                .ThenInclude(nt => nt.IAttributeTypeModel)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.Node)
                .ThenInclude(rn => rn.NodeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.TargetNode)
                .Where(n => !n.IsArchived);
        }
    }
}
