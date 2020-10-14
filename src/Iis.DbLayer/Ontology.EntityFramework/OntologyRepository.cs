using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Iis.DataModel;
using Iis.Domain;

using IIS.Repository;

using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class OntologyRepository : RepositoryBase<OntologyContext>, IOntologyRepository
    {
        public NodeEntity GetNodeEntityById(Guid id)
        {
            return Context.Nodes.FirstOrDefault(_ => _.Id == id);
        }
        public NodeEntity GetActiveNodeEntityById(Guid id)
        {
            return Context.Nodes
                .Include(p => p.NodeType)
                .FirstOrDefault(p => p.Id == id && !p.IsArchived && !p.NodeType.IsArchived);
        }

        public async Task<NodeEntity> GetNodeEntityByIdAsync(Guid id)
        {
            return await Context.Nodes.FirstOrDefaultAsync(_ => _.Id == id);
        }

        private IQueryable<NodeEntity> GetNodeEntityWithIncludesQuery()
        {
            return Context.Nodes
                .Include(n => n.Attribute)
                .Include(n => n.NodeType)
                .ThenInclude(nt => nt.AttributeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.Node)
                .ThenInclude(rn => rn.NodeType)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.TargetNode)
                .ThenInclude(tn => tn.NodeType)
                .Where(n => !n.IsArchived && !n.NodeType.IsArchived);
        }
        public async Task<NodeEntity> GetNodeEntityWithIncludesByIdAsync(Guid id)
        {
            var query = GetNodeEntityWithIncludesQuery();
            return await query.SingleOrDefaultAsync(n => !n.IsArchived && n.Id == id);
        }
        public NodeEntity GetNodeEntityWithIncludesById(Guid id)
        {
            var query = GetNodeEntityWithIncludesQuery();
            return query.SingleOrDefault(n => !n.IsArchived && n.Id == id);
        }

        public Task<List<NodeEntity>> GetNodeEntitiesByIdsAsync(IEnumerable<Guid> ids)
        {
            return Context.Nodes
                .Where(node => !node.IsArchived && !node.NodeType.IsArchived && ids.Contains(node.Id)).ToListAsync();
        }

        public Task<List<RelationEntity>> GetSourceRelationByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Context.Relations.Where(e => e.SourceNodeId == id && !e.Node.IsArchived)
                .Include(e => e.Node)
                .ThenInclude(e => e.NodeType)
                .Include(e => e.TargetNode)
                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .ToListAsync(cancellationToken);
        }

        public Task<List<RelationEntity>> GetAllRelationsAsync(Guid nodeId)
        {
            return Context.Relations
                .Include(r => r.Node)
                .Where(r => !r.Node.IsArchived && (r.TargetNodeId == nodeId || r.SourceNodeId == nodeId))
                .ToListAsync();
        }
        public void AddNode(NodeEntity nodeEntity)
        {
            Context.Nodes.Add(nodeEntity);
        }

        public void AddNodes(IEnumerable<NodeEntity> nodeEntities)
        {
            Context.AddRange(nodeEntities);
        }

        public void UpdateNodes(IEnumerable<NodeEntity> nodeEntities)
        {
            Context.Nodes.UpdateRange(nodeEntities);
        }

        public Task<List<RelationEntity>> GetDirectRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds)
        {
            var relationsQ = Context.Relations
                .Include(e => e.Node)
                    .ThenInclude(e => e.NodeType)
                .Include(e => e.TargetNode)
                    .ThenInclude(e => e.Attribute)
                .Include(e => e.SourceNode)
                .Where(e => nodeIds.Contains(e.SourceNodeId) 
                    && !e.Node.IsArchived
                    && !e.Node.NodeType.IsArchived
                    && !e.TargetNode.IsArchived
                    && !e.SourceNode.IsArchived
                    && !e.TargetNode.NodeType.IsArchived
                    && !e.SourceNode.NodeType.IsArchived
                    );
            if (relationIds != null)
                relationsQ = relationsQ.Where(e => relationIds.Contains(e.Node.NodeTypeId));
            return relationsQ.ToListAsync();
        }
        public Task<List<RelationEntity>> GetInversedRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds)
        {
            var relationsQ = Context.Relations
                .Include(e => e.Node)
                .ThenInclude(e => e.NodeType)
                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .Include(e => e.SourceNode).ThenInclude(e => e.Attribute)
                .Where(e => nodeIds.Contains(e.TargetNodeId) 
                    && !e.Node.IsArchived 
                    && !e.Node.NodeType.IsArchived
                    && !e.TargetNode.IsArchived
                    && !e.SourceNode.IsArchived
                    && !e.SourceNode.NodeType.IsArchived
                    && !e.TargetNode.NodeType.IsArchived);
            if (relationIds != null)
                relationsQ = relationsQ.Where(e => relationIds.Contains(e.Node.NodeTypeId));
            return relationsQ.ToListAsync();
        }

        public AttributeEntity GetAttributeEntityById(Guid id)
        {
            return Context.Attributes.SingleOrDefault(_ => _.Id == id);
        }

        public Task<List<NodeEntity>> GetNodesWithSuggestionAsync(IEnumerable<Guid> derived, ElasticFilter filter)
        {
            var query = string.IsNullOrWhiteSpace(filter.Suggestion)
                ? Context.Nodes.Where(e => derived.Contains(e.NodeTypeId) && !e.IsArchived)
                : Context.Relations
                    .Include(e => e.SourceNode)
                    .Where(e => derived.Contains(e.SourceNode.NodeTypeId) && !e.Node.IsArchived && !e.SourceNode.IsArchived)
                    .Where(e => EF.Functions.ILike(e.TargetNode.Attribute.Value, $"%{filter.Suggestion}%"))
                    .Select(e => e.SourceNode);

            return query
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Skip(filter.Offset)
                .Take(filter.Limit)
                .ToListAsync();
        }

        public Task<int> GetNodesCountWithSuggestionAsync(IEnumerable<Guid> derived, string suggestion)
        {
            if (suggestion == null)
            {
                return Context.Nodes.Where(e => derived.Contains(e.NodeTypeId) && !e.IsArchived).Distinct().CountAsync();
            }

            var relationsQ = Context.Relations
                .Include(e => e.SourceNode)
                .Where(e => derived.Contains(e.SourceNode.NodeTypeId) && !e.Node.IsArchived && !e.SourceNode.IsArchived)
                .Where(e =>
                    EF.Functions.ILike(e.TargetNode.Attribute.Value, $"%{suggestion}%"));
            return relationsQ.Select(e => e.SourceNode).Distinct().CountAsync();
        }
        public Task<List<Guid>> GetNodeIdListByFeatureIdListAsync(IEnumerable<Guid> featureIdList)
        {
            return Context.Relations
                .Where(e => featureIdList.Contains(e.TargetNodeId) && !e.Node.IsArchived)
                .Select(e => e.SourceNodeId)
                .ToListAsync();
        }

        public async Task<List<AttributeEntity>> GetAttributesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName, int limit)
        {
            return await
                (from ns in Context.Nodes
                 join r in Context.Relations on ns.Id equals r.SourceNodeId
                 join n in Context.Nodes on r.Id equals n.Id
                 join nt in Context.Nodes on r.TargetNodeId equals nt.Id
                 join a in Context.Attributes on nt.Id equals a.Id
                 join ntt in Context.NodeTypes on nt.NodeTypeId equals ntt.Id
                 where ns.NodeTypeId == nodeTypeId
                       && !ns.IsArchived && !nt.IsArchived && !n.IsArchived
                       && ntt.Name == valueTypeName
                       && (a.Value.StartsWith(value))
                 select a).Take(limit).ToListAsync();

        }

        public async Task<List<NodeEntity>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
        {
            return await
                (from n in Context.Nodes
                 join r in Context.Relations on n.Id equals r.SourceNodeId
                 join n2 in Context.Nodes on r.TargetNodeId equals n2.Id
                 join a in Context.Attributes on n2.Id equals a.Id
                 join nt in Context.NodeTypes on n2.NodeTypeId equals nt.Id
                 where n.NodeTypeId == nodeTypeId
                       && !n.IsArchived && !n2.IsArchived
                       && nt.Name == valueTypeName
                       && (a.Value == value || value == null)
                 select n).Distinct().ToListAsync();
        }
        public async Task<NodeEntity> UpdateNodeAsync(Guid id, Action<NodeEntity> action)
        {
            var nodeEntity = await Context.Nodes.Where(n => n.Id == id)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.Node)
                .Include(n => n.OutgoingRelations)
                .ThenInclude(r => r.TargetNode)
                .SingleOrDefaultAsync();

            if (nodeEntity == null) return null;

            action(nodeEntity);

            Context.Nodes.Update(nodeEntity);
            return nodeEntity;
        }

        public Task<List<Guid>> GetSourceNodeIdByTargetNodeId(Guid? propertyId, Guid targetNodeId)
        {
            return Context.Relations
                .AsNoTracking()
                .Include(p => p.Node)
                .Where(p => p.TargetNodeId == targetNodeId && p.Node.NodeTypeId == propertyId && !p.Node.IsArchived)
                .Select(p => p.SourceNodeId)
                .ToListAsync();
        }

        public Task<List<RelationEntity>> GetIncomingRelationsAsync(Guid entityId)
        {
            return Context.Relations
                .AsNoTracking()
                .Include(p => p.Node)
                .ThenInclude(p => p.NodeType)
                .Include(p => p.SourceNode)
                .ThenInclude(p => p.NodeType)
                .Where(p => p.TargetNodeId == entityId 
                    && !p.Node.IsArchived 
                    && !p.SourceNode.IsArchived 
                    && !p.SourceNode.NodeType.IsArchived)
                .ToListAsync();
        }

        public Task<List<RelationEntity>> GetIncomingRelationsAsync(IReadOnlyCollection<Guid> entityIds)
        {
            return Context.Relations
                .AsNoTracking()
                .Include(p => p.Node)
                .ThenInclude(p => p.NodeType)
                .Include(p => p.SourceNode)
                .ThenInclude(p => p.NodeType)
                .Where(p => entityIds.Contains(p.TargetNodeId) 
                    && !p.Node.IsArchived
                    && !p.SourceNode.IsArchived
                    && !p.SourceNode.NodeType.IsArchived)
                .ToListAsync();
        }

        public List<NodeEntity> GetAllNodes()
        {
            return Context.Nodes.AsNoTracking().ToList();
        }

        public List<RelationEntity> GetAllRelations()
        {
            return Context.Relations.AsNoTracking().ToList();
        }

        public List<AttributeEntity> GetAllAttributes()
        {
            return Context.Attributes.AsNoTracking().ToList();
        }

        public string GetAttributeValueByDotName(Guid id, string dotName)
        {
            var parts = dotName.Split('.');
            var nodeEntity = GetNodeEntityWithIncludesById(id);
            foreach (var part in parts)
            {
                if (nodeEntity == null) return null;
                var relation = nodeEntity.OutgoingRelations
                    .SingleOrDefault(r => r.Node.NodeType.Name == part);
                if (relation == null) return null;

                nodeEntity =  GetNodeEntityWithIncludesById(relation.TargetNodeId);
            }
            return nodeEntity?.Attribute?.Value;
        }
    }
}
