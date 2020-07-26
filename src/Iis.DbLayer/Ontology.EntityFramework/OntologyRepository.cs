using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;
using IIS.Repository;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public interface IOntologyRepository
    {
        NodeEntity GetNodeEntityById(Guid id);
        AttributeEntity GetAttributeEntityById(Guid id);
        Task<List<NodeEntity>> GetNodeEntitiesByIdsAsync(IEnumerable<Guid> ids);
        Task<List<RelationEntity>> GetSourceRelationByIdAsync(Guid id, CancellationToken cancellationToken);
        void AddNode(NodeEntity nodeEntity);
        void AddNodes(IEnumerable<NodeEntity> nodeEntities);
        void UpdateNodes(IEnumerable<NodeEntity> nodeEntities);
        Task<List<RelationEntity>> GetDirectRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds);
        Task<List<RelationEntity>> GetInversedRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds);
        Task<Dictionary<Guid, NodeEntity>> GetExistingNodes(CancellationToken cancellationToken);
        Task<int> GetNodesCountAsync(IEnumerable<Guid> derived);
        Task<int> GetNodesCountWithSuggestionAsync(IEnumerable<Guid> derived, string suggestion);
        Task<List<RelationEntity>> GetAllRelationsAsync(Guid nodeId);
        Task<List<Guid>> GetNodeIdListByFeatureIdListAsync(IEnumerable<Guid> featureIdList);
        Task<List<NodeEntity>> GetNodesAsync(IEnumerable<Guid> derived, ElasticFilter filter);
        Task<List<NodeEntity>> GetNodesWithSuggestionAsync(IEnumerable<Guid> derived, string suggestion, ElasticFilter filter);

        Task<IEnumerable<AttributeEntity>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName,
            int limit);

        Task<List<NodeEntity>> GetNodeByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
    }
    public class OntologyRepository : RepositoryBase<OntologyContext>, IOntologyRepository
    {
        public NodeEntity GetNodeEntityById(Guid id)
        {
            return Context.Nodes.FirstOrDefault(_ => _.Id == id);
        }

        public Task<List<NodeEntity>> GetNodeEntitiesByIdsAsync(IEnumerable<Guid> ids)
        {
            return Context.Nodes.Where(node => !node.IsArchived && ids.Contains(node.Id)).ToListAsync();
        }

        public Task<List<RelationEntity>> GetSourceRelationByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Context.Relations.Where(e => e.SourceNodeId == id && !e.Node.IsArchived)
                .Include(e => e.Node)
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
                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .Where(e => nodeIds.Contains(e.SourceNodeId) && !e.Node.IsArchived);
            if (relationIds != null)
                relationsQ = relationsQ.Where(e => relationIds.Contains(e.Node.NodeTypeId));
            return relationsQ.ToListAsync();
        }
        public Task<List<RelationEntity>> GetInversedRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds)
        {
            var relationsQ = Context.Relations
                .Include(e => e.Node)
                .Include(e => e.SourceNode).ThenInclude(e => e.Attribute)
                .Where(e => nodeIds.Contains(e.TargetNodeId) && !e.Node.IsArchived);
            if (relationIds != null)
                relationsQ = relationsQ.Where(e => relationIds.Contains(e.Node.NodeTypeId));
            return relationsQ.ToListAsync();
        }

        public async Task<Dictionary<Guid, NodeEntity>> GetExistingNodes(CancellationToken cancellationToken)
        {
            IQueryable<NodeEntity> query =
                from node in Context.Nodes
                    .Include(x => x.Relation)
                    .Include(x => x.Attribute)
                    .Include(x => x.OutgoingRelations)
                    .ThenInclude(x => x.Node)
                select node;
            Dictionary<Guid, NodeEntity>
                existingNodes = await query.ToDictionaryAsync(x => x.Id, cancellationToken);
            return existingNodes;
        }

        public AttributeEntity GetAttributeEntityById(Guid id)
        {
            return Context.Attributes.SingleOrDefault(_ => _.Id == id);
        }

        public Task<List<NodeEntity>> GetNodesWithSuggestionAsync(IEnumerable<Guid> derived, string suggestion, ElasticFilter filter)
        {
            var relationsQ = Context.Relations
                .Include(e => e.SourceNode)
                .Where(e => derived.Contains(e.SourceNode.NodeTypeId) && !e.Node.IsArchived && !e.SourceNode.IsArchived);
            if (suggestion != null)
                relationsQ = relationsQ.Where(e =>
                    EF.Functions.ILike(e.TargetNode.Attribute.Value, $"%{suggestion}%"));
            return relationsQ.Select(e => e.SourceNode).Skip(filter.Offset).Take(filter.Limit).ToListAsync();
        }

        public Task<List<NodeEntity>> GetNodesAsync(IEnumerable<Guid> derived, ElasticFilter filter)
        {
            return Context.Nodes.Where(e => derived.Contains(e.NodeTypeId) && !e.IsArchived)
                .Skip(filter.Offset).Take(filter.Limit).ToListAsync();
        }

        public Task<int> GetNodesCountAsync(IEnumerable<Guid> derived)
        {
            return Context.Nodes.Where(e => derived.Contains(e.NodeTypeId) && !e.IsArchived).Distinct().CountAsync();
        }

        public Task<int> GetNodesCountWithSuggestionAsync(IEnumerable<Guid> derived, string suggestion)
        {
            var relationsQ = Context.Relations
                .Include(e => e.SourceNode)
                .Where(e => derived.Contains(e.SourceNode.NodeTypeId) && !e.Node.IsArchived && !e.SourceNode.IsArchived);
            if (suggestion != null)
                relationsQ = relationsQ.Where(e =>
                    EF.Functions.ILike(e.TargetNode.Attribute.Value, $"%{suggestion}%"));
            return relationsQ.Select(e => e.SourceNode).Distinct().CountAsync();
        }
        public Task<List<Guid>> GetNodeIdListByFeatureIdListAsync(IEnumerable<Guid> featureIdList)
        {
            return Context.Relations
                .Where(e => featureIdList.Contains(e.TargetNodeId))
                .Select(e => e.SourceNodeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttributeEntity>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName, int limit)
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
                       && (a.Value.StartsWith(value))
                 select a).Take(limit).ToListAsync();

        }

        public async Task<List<NodeEntity>> GetNodeByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
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
                 select n).ToListAsync();
        }
    }
}
