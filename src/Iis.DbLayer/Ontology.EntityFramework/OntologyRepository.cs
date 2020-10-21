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
        public Task<List<RelationEntity>> GetIncomingRelationsAsync(IEnumerable<Guid> entityIdList, IEnumerable<string> relationTypeNameList)
        {
            return Context.Relations
                    .Include(p => p.Node)
                    .ThenInclude(p => p.NodeType)
                    .Where(p => entityIdList.Contains(p.TargetNodeId) && relationTypeNameList.Contains(p.Node.NodeType.Name))
                    .AsNoTracking()
                    .ToListAsync();
        }
    }
}
