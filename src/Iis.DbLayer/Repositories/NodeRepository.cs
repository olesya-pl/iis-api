using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories.Helpers;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
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

        public async Task<List<ElasticBulkResponse>> PutNodesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var flattenNodes = new List<FlattenNodeResult>(ids.Count());
            foreach (var id in ids)
            {
                flattenNodes.Add(await _nodeFlattener.FlattenNode(id, cancellationToken));
            }

            var documentsByIndex = flattenNodes.GroupBy(x => x.NodeTypeName).Select(x => new
            {
                Index = x.Key,
                Documents = x.Aggregate("", (acc, p) => acc += $"{{ \"index\":{{ \"_id\": \"{p.Id:N}\" }} }}\n{p.SerializedNode.RemoveWhiteSpace()}\n")
            });

            var result = new List<ElasticBulkResponse>();
            foreach (var item in documentsByIndex)
            {
                var response = await _elasticManager.PutsDocumentsAsync(item.Index, item.Documents, cancellationToken);
                result.AddRange(response);
            }

            return result;
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

        private List<FlattenNodeResult> GetHistoricalNodes(FlattenNodeResult node, IEnumerable<IChangeHistoryItem> changes) 
        {
            var changesByRequestId = changes
                    .GroupBy(x => x.RequestId)
                    .Where(x => x.Count() > 0)
                    .OrderByDescending(x => x.First().Date)
                    .ToList();

            var chistoricalNodes = new List<FlattenNodeResult>(changes.Count());
            var actualNode = node;
            foreach (var changePack in changesByRequestId) 
            {
                var olderNode = new FlattenNodeResult
                {
                    Id = actualNode.Id,
                    NodeTypeName = actualNode.NodeTypeName,
                    SerializedNode = actualNode.SerializedNode.ReplaceOrAddValues(changePack.Select(x => (x.PropertyName, x.OldValue)).ToArray())
                };
                chistoricalNodes.Add(olderNode);
                actualNode = olderNode;
            }

            return chistoricalNodes;
        }

        public async Task<bool> PutHistoricalNodesAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var actualNodes = new List<FlattenNodeResult>(ids.Count());
            foreach (var id in ids)
            {
                actualNodes.Add(await _nodeFlattener.FlattenNode(id));
            }

            var nodeChanges = await _changeHistoryService.GetChangeHistory(ids);
            var nodes = new List<FlattenNodeResult>();
            foreach (var actualNode in actualNodes)
            {
                var changes = nodeChanges
                    .Where(x => x.TargetId == Guid.Parse(actualNode.Id))
                    .GroupBy(x => x.RequestId)
                    .Where(x => x.Count() > 0)
                    .OrderByDescending(x => x.First().Date)
                    .ToList();

                var currentNode = actualNode;
                foreach (var changePack in changes)
                {
                    var olderNode = new FlattenNodeResult
                    {
                        Id = currentNode.Id,
                        NodeTypeName = currentNode.NodeTypeName,
                        SerializedNode = currentNode.SerializedNode.ReplaceOrAddValues(changePack.Select(x => (x.PropertyName, x.OldValue)).ToArray())
                    };
                    nodes.Add(olderNode);
                    currentNode = olderNode;
                }
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
    }
}
