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
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using Iis.Utility;
using MoreLinq;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories
{
    public class NodeRepository : INodeRepository
    {
        private readonly IElasticManager _elasticManager;
        private readonly NodeFlattener _nodeFlattener;
        private readonly IChangeHistoryService _changeHistoryService;
        private const int BulkSize = 50000;
        private static List<string> PropertiesToIgnore = new List<string>() { "photo", "lastConfirmedAt", "attachment" };

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
            var result = _nodeFlattener.FlattenNode(id);
            return await _elasticManager.PutDocumentAsync(
                result.NodeTypeName,
                result.Id,
                result.SerializedNode, cancellationToken);
        }

        public async Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExclude, CancellationToken cancellationToken = default)
        {
            var result = _nodeFlattener.FlattenNode(id);
            result.SerializedNode = ExcludeFields(result.SerializedNode, fieldsToExclude);

            return await _elasticManager.PutDocumentAsync(
                result.NodeTypeName,
                result.Id,
                result.SerializedNode, cancellationToken);
        }

        public Task<List<ElasticBulkResponse>> PutNodesAsync(IReadOnlyCollection<INode> nodes, CancellationToken ct)
        {
            var flattenNodes = _nodeFlattener.FlattenNodes(nodes);
            return PutNodesToElasticAsync(flattenNodes, false, ct);
        }

        public Task<List<ElasticBulkResponse>> PutNodesAsync(IReadOnlyCollection<INode> nodes, IEnumerable<string> fieldsToExclude, CancellationToken ct)
        {
            var flattenNodes = _nodeFlattener.FlattenNodes(nodes);
            flattenNodes.ForEach(node =>
            {
                node.SerializedNode = ExcludeFields(node.SerializedNode, fieldsToExclude);
            });

            return PutNodesToElasticAsync(flattenNodes, false, ct);
        }

        public async Task<List<ElasticBulkResponse>> PutHistoricalNodesAsync(IReadOnlyCollection<INode> items, CancellationToken ct = default)
        {
            var nodes = _nodeFlattener.FlattenNodes(items);
            var changes = await _changeHistoryService.GetChangeHistory(items.Select(x => x.Id).Distinct().ToList());

            var historicalNodes = nodes.SelectMany(x => GetHistoricalNodes(x, changes.Where(ch => ch.TargetId.ToString("N") == x.Id)));

            return await PutNodesToElasticAsync(historicalNodes, true, ct);
        }

        public async Task<List<ElasticBulkResponse>> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default)
        {
            var actualNode = _nodeFlattener.FlattenNode(id);
            var getNodeChanges = requestId.HasValue ?
                _changeHistoryService.GetChangeHistoryByRequest(requestId.Value) :
                _changeHistoryService.GetChangeHistoryAsync(new ChangeHistoryParams
                {
                    EntityIdentityList = new[] { id }
                });

            var nodeChanges = await getNodeChanges;

            var nodes = GetHistoricalNodes(actualNode, nodeChanges);

            return await PutNodesToElasticAsync(nodes, true, ct);
        }

        private async Task<List<ElasticBulkResponse>> PutNodesToElasticAsync(IEnumerable<FlattenNodeResult> nodes, bool isHistoricalIndex, CancellationToken ct = default)
        {
            var responses = new List<ElasticBulkResponse>(nodes.Count());
            foreach (var group in nodes.GroupBy(x => x.NodeTypeName))
            {
                var index = isHistoricalIndex ? $"historical_{group.Key}" : group.Key;
                foreach (var nodeBatch in group.Batch(BulkSize))
                {
                    ct.ThrowIfCancellationRequested();

                    var bulkData = GenerateBulkData(nodeBatch, isHistoricalIndex);
                    var response = await _elasticManager.PutDocumentsAsync(index, bulkData, ct);

                    responses.AddRange(response);
                }
            }

            return responses;
        }

        private List<FlattenNodeResult> GetHistoricalNodes(FlattenNodeResult node, IEnumerable<ChangeHistoryDto> changes)
        {
            var changesByRequestId = changes
                    .Where(x => x.TargetId.ToString("N") == node.Id && !string.IsNullOrWhiteSpace(x.OldValue) && !PropertiesToIgnore.Contains(x.PropertyName))
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

        private string GenerateBulkData(IEnumerable<FlattenNodeResult> nodes, bool isHistoricalIndex)
        {
            Func<string, string> getIdFunc = id => isHistoricalIndex ? Guid.NewGuid().ToString("N") : id;
            return nodes.Aggregate("", (acc, p) => acc += $"{{ \"index\":{{ \"_id\": \"{getIdFunc(p.Id):N}\" }} }}\n{p.SerializedNode.RemoveNewLineCharacters()}\n");
        }

        private string ExcludeFields(string json, IEnumerable<string> fieldsToExclude)
        {
            var jObject = JObject.Parse(json);
            foreach (var field in fieldsToExclude)
            {
                var propertyToExclude = jObject.Property(field, StringComparison.OrdinalIgnoreCase);
                propertyToExclude?.Remove();
            }

            return jObject.ToString();
        }
    }
}
