﻿using Iis.DataModel;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class ExtNodeService : IExtNodeService
    {
        private readonly OntologyContext _context;
        private List<Guid> _objectOfStudyTypes;
        private List<Guid> ObjectOfStudyTypes
        {
            get
            {
                return _objectOfStudyTypes ?? (_objectOfStudyTypes = GetObjectOfStudyTypes());
            }
        }

        public ExtNodeService(OntologyContext context)
        {
            _context = context;
        }

        private List<Guid> GetObjectOfStudyTypes()
        {
            //TODO:
            var objectOfStudyType = _context.NodeTypes
                .Include(nt => nt.IncomingRelations)
                .Where(nt => nt.Name == "ObjectOfStudy" && nt.Kind == Kind.Entity)
                .SingleOrDefault();

            if(objectOfStudyType != null)
            {
                return objectOfStudyType.IncomingRelations
                    .Where(r => r.Kind == RelationKind.Inheritance)
                    .Select(r => r.SourceTypeId)
                    .ToList();
            }
            else
            //TODO: we need some staff to do a check Contour or Odyssey
            {
                var types = _context.NodeTypes
                                .Include(nt => nt.IncomingRelations)
                                .Where(nt => nt.Kind == Kind.Entity && (nt.Name == "Organization" || nt.Name == "Person"))
                                .ToList();

                return types.SelectMany(t => t.IncomingRelations)
                            .Select(r => r.SourceTypeId)
                            .ToList();
            }
        }

        private object GetAttributeValue(NodeEntity nodeEntity)
        {
            if (nodeEntity.Attribute == null) return null;

            var scalarType = nodeEntity.INodeTypeModel.AttributeType.ScalarType;
            var value = nodeEntity.Attribute.Value;
            switch (scalarType)
            {
                case ScalarType.Int:
                    return Convert.ToInt32(value);
                case ScalarType.Date:
                    return Convert.ToDateTime(value);
                default:
                    return value.ToString();
            }
        }

        private async Task<ExtNode> MapExtNodeAsync(NodeEntity nodeEntity, string nodeTypeName, string nodeTypeTitle, CancellationToken cancellationToken = default)
        {
            var extNode = new ExtNode
            {
                Id = nodeEntity.Id.ToString("N"),
                NodeTypeId = nodeEntity.NodeTypeId.ToString("N"),
                NodeTypeName = nodeTypeName,
                NodeTypeTitle = nodeTypeTitle,
                EntityTypeName = nodeEntity.INodeTypeModel.Name,
                AttributeValue = GetAttributeValue(nodeEntity),
                ScalarType = nodeEntity.INodeTypeModel?.AttributeType?.ScalarType,
                CreatedAt = nodeEntity.CreatedAt,
                UpdatedAt = nodeEntity.UpdatedAt,
                Children = await GetExtNodesByRelations(nodeEntity.OutgoingRelations, cancellationToken)
            };
            return await Task.FromResult(extNode);
        }

        public async Task<IExtNode> GetExtNodeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var nodeEntity = await GetNodeQuery()
                .Where(n => n.Id == id)
                .SingleOrDefaultAsync();
            
            if (nodeEntity == null) return null;

            var extNode = await MapExtNodeAsync(nodeEntity, nodeEntity.INodeTypeModel.Name, nodeEntity.INodeTypeModel.Title, cancellationToken);
            return extNode;
        }

        private async Task<List<IExtNode>> GetExtNodesByRelations(IEnumerable<RelationEntity> relations, CancellationToken cancellationToken = default)
        {
            var result = new List<IExtNode>();
            foreach (var relation in relations.Where(r => !r.Node.IsArchived && !r.TargetNode.IsArchived))
            {
                var node = await GetNodeQuery().Where(node => node.Id == relation.TargetNodeId).SingleOrDefaultAsync();
                if (!ObjectOfStudyTypes.Contains(node.NodeTypeId))
                {
                    var extNode = await MapExtNodeAsync(node, relation.Node.INodeTypeModel.Name, relation.Node.INodeTypeModel.Title ,cancellationToken);
                    result.Add(extNode);
                }
            }
            return result;
        }

        public async Task<List<IExtNode>> GetExtNodesByTypeIdsAsync(IEnumerable<string> typeNames, CancellationToken cancellationToken = default)
        {
            var typeIds = await _context.NodeTypes.Where(nt => typeNames.Contains(nt.Name)).Select(nt => nt.Id).ToListAsync();

            var nodes = await GetNodeQuery()
                .Where(node => typeIds.Contains(node.NodeTypeId))
                .ToListAsync();

            int cnt = 0, total = nodes.Count();
            var result = new List<IExtNode>();

            foreach (var node in nodes)
            {
                Console.WriteLine($"{++cnt}/{total}: {node.Id};{node.INodeTypeModel.Name}");
                var extNode = await GetExtNodeByIdAsync(node.Id, cancellationToken);
                result.Add(extNode);
            }

            return result;
        }

        private IQueryable<NodeEntity> GetNodeQuery()
        {
            return _context.Nodes
                .Include(n => n.Attribute)
                .Include(n => n.INodeTypeModel)
                .ThenInclude(nt => nt.AttributeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.Node)
                .ThenInclude(rn => rn.INodeTypeModel)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.TargetNode)
                .Where(n => !n.IsArchived);
        }
    }
}
