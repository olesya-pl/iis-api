using Iis.DataModel;
using Iis.DbLayer.Repositories;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;

using IIS.Repository;
using IIS.Repository.Factories;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        private readonly FileUrlGetter _fileUrlGetter;

        public ExtNodeService(OntologyContext context,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IOntologySchema ontologySchema,
            FileUrlGetter fileUrlGetter) : base(unitOfWorkFactory)
        {
            _context = context;
            _ontologySchema = ontologySchema;
            _fileUrlGetter = fileUrlGetter;
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

        public async Task<IExtNode> GetExtNodeWithoutNestedObjectsAsync(Guid id, CancellationToken ct = default)
        {
            var nodeEntity = await RunWithoutCommitAsync(async uow => await uow.OntologyRepository.GetNodeEntityWithIncludesByIdAsync(id));

            if (nodeEntity == null) return null;

            var extNode = await MapExtNodeWithoutNestedObjectsAsync(
                nodeEntity,
                nodeEntity.NodeType.Name,
                nodeEntity.NodeType.Title,
                ct);
            return extNode;
        }

        public IExtNode GetExtNode(INode nodeEntity)
        {
            var extNode = MapExtNode(
                nodeEntity,
                nodeEntity.NodeType.Name,
                nodeEntity.NodeType.Title,
                new List<Guid> { nodeEntity.Id });
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
            var extNode = MapExtNodeBase(nodeEntity, nodeTypeName, nodeTypeTitle);
            extNode.EntityTypeName = nodeEntity.NodeType.Name;
            extNode.AttributeValue = GetAttributeValue(nodeEntity);
            extNode.ScalarType = nodeEntity.NodeType?.IAttributeTypeModel?.ScalarType;
            extNode.Children = await GetExtNodesByRelations(nodeEntity.OutgoingRelations, visitedEntityIds, cancellationToken);
            return extNode;
        }

        private async Task<ExtNode> MapExtNodeWithoutNestedObjectsAsync(
            NodeEntity nodeEntity,
            string nodeTypeName,
            string nodeTypeTitle,
            CancellationToken cancellationToken = default)
        {
            var extNode = MapExtNodeBase(nodeEntity, nodeTypeName, nodeTypeTitle);
            extNode.EntityTypeName = nodeEntity.NodeType.Name;
            extNode.AttributeValue = GetAttributeValue(nodeEntity);
            extNode.ScalarType = nodeEntity.NodeType?.IAttributeTypeModel?.ScalarType;
            extNode.Children = await GetExtNodesByRelationsWithoutNestedObjects(nodeEntity.OutgoingRelations, cancellationToken);
            return extNode;
        }

        private ExtNode MapExtNode(
            INode nodeEntity,
            string nodeTypeName,
            string nodeTypeTitle,
            List<Guid> visitedEntityIds)
        {
            if (nodeEntity.NodeType.IsObjectOfStudy)
            {
                visitedEntityIds.Add(nodeEntity.Id);
            }
            var extNode = MapExtNodeBase(nodeEntity, nodeTypeName, nodeTypeTitle);
            extNode.EntityTypeName = nodeEntity.NodeType.Name;
            extNode.AttributeValue = GetAttributeValue(nodeEntity);
            extNode.ScalarType = nodeEntity.NodeType?.AttributeType?.ScalarType;
            extNode.Children = GetExtNodesByRelations(nodeEntity.OutgoingRelations, visitedEntityIds);
            return extNode;
        }

        private ExtNode MapExtNodeBase(
            INodeBase nodeEntity,
            string nodeTypeName,
            string nodeTypeTitle)
        {
            return new ExtNode
            {
                Id = nodeEntity.Id.ToString("N"),
                NodeTypeId = nodeEntity.NodeTypeId.ToString("N"),
                NodeType = _ontologySchema.GetNodeTypeById(nodeEntity.NodeTypeId),
                NodeTypeName = nodeTypeName,
                NodeTypeTitle = nodeTypeTitle,
                CreatedAt = nodeEntity.CreatedAt,
                UpdatedAt = nodeEntity.UpdatedAt
            };
        }

        private object GetAttributeValue(NodeEntity nodeEntity)
        {
            if (nodeEntity.Attribute == null) return null;

            var scalarType = nodeEntity.NodeType.IAttributeTypeModel.ScalarType;
            var value = nodeEntity.Attribute.Value;
            return FormatValue(scalarType, value);
        }

        private object GetAttributeValue(INode nodeEntity)
        {
            if (nodeEntity.Attribute == null) return null;

            var scalarType = nodeEntity.NodeType.AttributeType.ScalarType;
            var value = nodeEntity.Attribute.Value;
            return FormatValue(scalarType, value);
        }

        private object FormatValue(ScalarType scalarType, string value)
        {
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
                case ScalarType.File:
                    return new
                    {
                        fileId = value,
                        url = _fileUrlGetter.GetFileUrl(new Guid(value))
                    };
                case ScalarType.IntegerRange:
                case ScalarType.FloatRange:
                    return FormatRange(value);
                default:
                    return value;
            }
        }

        private object FormatRange(string value)
        {
            var splitted = value.Split('-', ' ', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Count() == 1 || splitted.Count() == 2)
            {
                var firstString = splitted.First();
                var lastString = splitted.Last();

                if (decimal.TryParse(firstString, out var first) && decimal.TryParse(lastString, out var last))
                {
                    return new
                    {
                        gte = first,
                        lte = last
                    };
                }             
            }
            return null;
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

        private async Task<List<ExtNode>> GetExtNodesByRelationsWithoutNestedObjects(
            IEnumerable<RelationEntity> relations,
            CancellationToken cancellationToken = default)
        {
            var result = new List<ExtNode>();
            foreach (var relation in relations.Where(r => !r.Node.IsArchived && !r.Node.NodeType.IsArchived
                && !r.TargetNode.IsArchived && !r.TargetNode.NodeType.IsArchived))
            {
                var node = await RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.OntologyRepository.GetNodeEntityWithIncludesByIdAsync(relation.TargetNodeId));
                if (!_ontologySchema.GetNodeTypeById(node.NodeTypeId).IsObjectOfStudy)
                {
                    var extNode = await MapExtNodeWithoutNestedObjectsAsync(
                        node,
                        relation.Node.NodeType.Name,
                        relation.Node.NodeType.Title,
                        cancellationToken);
                    result.Add(extNode);
                }
            }
            return result;
        }

        private List<ExtNode> GetExtNodesByRelations(
            IEnumerable<IRelation> relations,
            List<Guid> visitedEntityIds)
        {
            var result = new List<ExtNode>();
            foreach (var relation in relations.Where(r => !r.Node.IsArchived && !r.Node.NodeType.IsArchived
                && !r.TargetNode.IsArchived && !r.TargetNode.NodeType.IsArchived))
            {
                var node = relation.TargetNode;
                if (!visitedEntityIds.Contains(node.Id))
                {
                    var extNode = MapExtNode(
                        node,
                        relation.Node.NodeType.Name,
                        relation.Node.NodeType.Title,
                        visitedEntityIds);
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

        public List<IExtNode> GetExtNodes(IReadOnlyCollection<INode> itemsToUpdate)
        {
            return itemsToUpdate.Select(p => GetExtNode(p))
                .ToList();
        }
    }
}
