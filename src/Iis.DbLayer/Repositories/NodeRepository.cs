using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories.Helpers;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories
{
    public class NodeRepository : INodeRepository
    {
        private readonly IElasticManager _elasticManager;
        private readonly NodeFlattener _nodeFlattener;
        private readonly IChangeHistoryService _changeHistoryService;

        public NodeRepository(IElasticManager elasticManager,
            NodeFlattener nodeFlattener,
            IOntologySchema ontologySchema, IChangeHistoryService changeHistoryService)
        {
            _elasticManager = elasticManager;
            _nodeFlattener = nodeFlattener;
            _changeHistoryService = changeHistoryService;

            var objectOfStudyType = ontologySchema.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString());
            if (objectOfStudyType != null)
            {
                OntologyIndexes = objectOfStudyType.GetAllDescendants()
                    .Where(nt => !nt.IsAbstract)
                    .Select(nt => nt.Name)
                    .ToArray();
            }
        }

        public IReadOnlyCollection<string> OntologyIndexes { get; }

        public async Task<JObject> GetJsonNodeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _elasticManager.GetDocumentByIdAsync(OntologyIndexes, id.ToString("N"), cancellationToken);
            return result.Items.FirstOrDefault()?.SearchResult;
        }

        public async Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _nodeFlattener.FlattenNode(id, cancellationToken);
            return await _elasticManager.PutDocumentAsync(
                result.NodeTypeName,
                result.Id,
                result.SerializedNode, cancellationToken);
        }

        public async Task<bool> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default) 
        {
            var getActualNode = _nodeFlattener.FlattenNode(id, ct);
            var getNodeChanges = requestId.HasValue ? 
                _changeHistoryService.GetChangeHistoryByRequest(requestId.Value) : 
                _changeHistoryService.GetChangeHistory(id, null);
            
            await Task.WhenAll(getActualNode, getNodeChanges);

            var changes = getNodeChanges.Result
                .GroupBy(x => x.RequestId)
                .Where(x => x.Count() > 0)
                .OrderByDescending(x => x.First().Date)
                .ToList();

            var actualNode = getActualNode.Result;
            var nodes = new List<FlattenNodeResult>();
            foreach (var changePack in changes)
            {
                var olderNode = new FlattenNodeResult
                {
                    Id = actualNode.Id,
                    NodeTypeName = actualNode.NodeTypeName,
                    SerializedNode = actualNode.SerializedNode.ReplaceOrAddValues(changePack.Select(x => (x.PropertyName, x.OldValue)).ToArray())
                };
                nodes.Add(olderNode);
                actualNode = olderNode;
            }

            //TODO: should put all documents by one query
            foreach (var item in nodes)
            {
                var result = await _elasticManager.PutDocumentAsync(
                $"historical_{item.NodeTypeName}",
                Guid.NewGuid().ToString(),
                item.SerializedNode, ct);
            }

            return true;
        }

        public async Task<bool> PutNodesAsync(IReadOnlyCollection<INode> itemsToUpdate, CancellationToken cancellationToken)
        {
            var result = _nodeFlattener.FlattenNodes(itemsToUpdate);
            var putDocumentTasks = result.Select(p => _elasticManager.PutDocumentAsync(
                p.NodeTypeName,
                p.Id,
                p.SerializedNode, cancellationToken));
            await Task.WhenAll(putDocumentTasks);
            return true;            
        }

        public async Task<bool> PutHistoricalNodesAsync(IReadOnlyCollection<INode> items, CancellationToken ct = default)
        {
            var nodes = _nodeFlattener.FlattenNodes(items);
            var changes = await _changeHistoryService.GetChangeHistory(items.Select(x => x.Id).Distinct().ToList());

            var nodesToIndex = new List<FlattenNodeResult>();
            foreach (var node in nodes)
            {
                var currentNode = node;
                var nodeChanges = changes
                   .Where(x => x.TargetId.ToString("N") == node.Id)
                   .GroupBy(x => x.RequestId)
                   .Where(x => x.Count() > 0)
                   .OrderByDescending(x => x.First().Date)
                   .ToList();

                foreach (var changePack in nodeChanges)
                {
                    var olderNode = new FlattenNodeResult
                    {
                        Id = currentNode.Id,
                        NodeTypeName = currentNode.NodeTypeName,
                        SerializedNode = currentNode.SerializedNode.ReplaceOrAddValues(changePack.Select(x => (x.PropertyName, x.OldValue)).ToArray())
                    };
                    nodesToIndex.Add(olderNode);
                    currentNode = olderNode;
                }
            }

            //TODO: should put all documents by one query
            foreach (var item in nodesToIndex)
            {
                var result = await _elasticManager.PutDocumentAsync(
                $"historical_{item.NodeTypeName}",
                Guid.NewGuid().ToString(),
                item.SerializedNode, ct);
            }

            return true;
        }
    }
}
