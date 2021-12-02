using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Helpers;
using Iis.Services.Contracts.Interfaces;
using Iis.DbLayer.Repositories;
using Iis.Utility;
using MoreLinq;
using Newtonsoft.Json.Linq;

namespace Iis.Services
{
    public class NodeSaveService : INodeSaveService
    {
        private readonly IElasticManager _elasticManager;
        private readonly NodeFlattener<IIISUnitOfWork> _nodeFlattener;
        private const int BulkSize = 50000;

        public NodeSaveService(IElasticManager elasticManager,
            NodeFlattener<IIISUnitOfWork> nodeFlattener,
            IOntologySchema ontologySchema)
        {
            _elasticManager = elasticManager;
            _nodeFlattener = nodeFlattener;

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
            var result = await _nodeFlattener.FlattenNodeAsync(id);
            return await _elasticManager.PutDocumentAsync(
                result.NodeTypeName,
                result.Id,
                result.SerializedNode, cancellationToken);
        }

        public async Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExclude, CancellationToken cancellationToken = default)
        {
            var result = await _nodeFlattener.FlattenNodeAsync(id);
            result.SerializedNode = ExcludeFields(result.SerializedNode, fieldsToExclude);

            return await _elasticManager.PutDocumentAsync(
                result.NodeTypeName,
                result.Id,
                result.SerializedNode, cancellationToken);
        }

        public async Task<List<ElasticBulkResponse>> PutNodesAsync(IReadOnlyCollection<INode> nodes, CancellationToken ct)
        {
            var flattenNodes = await _nodeFlattener.FlattenNodesAsync(nodes);

            return await PutNodesToElasticAsync(flattenNodes, ct);
        }

        public async Task<List<ElasticBulkResponse>> PutNodesAsync(IReadOnlyCollection<INode> nodes, IEnumerable<string> fieldsToExclude, CancellationToken ct)
        {
            var flattenNodes = await _nodeFlattener.FlattenNodesAsync(nodes);
            flattenNodes.ForEach(node =>
            {
                node.SerializedNode = ExcludeFields(node.SerializedNode, fieldsToExclude);
            });

            return await PutNodesToElasticAsync(flattenNodes, ct);
        }

        private async Task<List<ElasticBulkResponse>> PutNodesToElasticAsync(IEnumerable<FlattenNodeResult> nodes, CancellationToken ct = default)
        {
            var responses = new List<ElasticBulkResponse>(nodes.Count());
            foreach (var group in nodes.GroupBy(x => x.NodeTypeName))
            {
                foreach (var nodeBatch in group.Batch(BulkSize))
                {
                    ct.ThrowIfCancellationRequested();

                    var bulkData = GenerateBulkData(nodeBatch);
                    var response = await _elasticManager.PutDocumentsAsync(group.Key, bulkData, ct: ct);

                    responses.AddRange(response);
                }
            }

            return responses;
        }

        private string GenerateBulkData(IEnumerable<FlattenNodeResult> nodes)
        {
            return nodes.Aggregate("", (acc, p) => acc += $"{{ \"index\":{{ \"_id\": \"{p.Id:N}\" }} }}\n{p.SerializedNode.RemoveNewLineCharacters()}\n");
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