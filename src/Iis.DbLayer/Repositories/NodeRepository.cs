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
                flattenNodes.Add(await _nodeFlattener.FlattenNode(id, cancellationToken));

            var bulkDataByNodeType = GenerateBulkData(flattenNodes);
            
            var result = new List<ElasticBulkResponse>();
            foreach (var item in bulkDataByNodeType)
            {
                var response = await _elasticManager.PutsDocumentsAsync(item.NodeType, item.Documents, cancellationToken);
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

            var historicalNodes = GetHistoricalNodes(getActualNode.Result, getNodeChanges.Result);


            var bulkDataByNodeType = GenerateBulkData(historicalNodes);

            var result = new List<ElasticBulkResponse>();
            foreach (var item in bulkDataByNodeType)
            {
                var response = await _elasticManager.PutsDocumentsAsync(ToHistoricalIndex(item.NodeType), item.Documents, ct);
                result.AddRange(response);
            }

            return result.All(x => x.IsSuccess);
        }

        public async Task<List<ElasticBulkResponse>> PutHistoricalNodesAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var actualNodes = new List<FlattenNodeResult>(ids.Count());
            foreach (var id in ids)
            {
                actualNodes.Add(await _nodeFlattener.FlattenNode(id));
            }

            var changes = await _changeHistoryService.GetChangeHistory(ids);
            var nodes = new List<FlattenNodeResult>();
            foreach (var actualNode in actualNodes)
            {
                var nodeChanges = changes
                    .Where(x => x.TargetId == Guid.Parse(actualNode.Id))
                    .ToList();

                nodes.AddRange(GetHistoricalNodes(actualNode, nodeChanges));
            }

            var bulkDataByNodeType = GenerateBulkData(nodes);

            var result = new List<ElasticBulkResponse>();
            foreach (var item in bulkDataByNodeType)
            {
                var response = await _elasticManager.PutsDocumentsAsync(ToHistoricalIndex(item.NodeType), item.Documents, ct);
                result.AddRange(response);
            }

            return result;
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

        private IEnumerable<(string NodeType, string Documents)> GenerateBulkData(IEnumerable<FlattenNodeResult> nodes)
        {
            return nodes.GroupBy(x => x.NodeTypeName).Select(x =>
            (
                x.Key,
                x.Aggregate("", (acc, p) => acc += $"{{ \"index\":{{ \"_id\": \"{p.Id:N}\" }} }}\n{p.SerializedNode.RemoveWhiteSpace()}\n")
            ));
        }

        private string ToHistoricalIndex(string nodeTypeName) 
        {
            return $"historical_{nodeTypeName}";
        }
    }
}
