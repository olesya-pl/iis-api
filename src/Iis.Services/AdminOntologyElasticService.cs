﻿using Iis.DbLayer.Repositories;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services
{
    public class AdminOntologyElasticService : IAdminOntologyElasticService
    {
        private readonly IElasticManager _elasticManager;
        private readonly IElasticState _elasticState;
        private readonly IOntologySchema _ontologySchema;
        private readonly IExtNodeService _extNodeService;
        private readonly INodeRepository _nodeRepository;
        private readonly OntologyNodesData ontologyNodesData;

        public StringBuilder Logger { get; set; }

        public AdminOntologyElasticService(
            IElasticState elasticState,
            IElasticManager elasticManager,
            IOntologySchema ontologySchema,
            IExtNodeService extNodeService,
            INodeRepository nodeRepository,
            OntologyNodesData ontologyNodesData)
        {
            _elasticState = elasticState ?? throw new ArgumentNullException(nameof(elasticState));
            _elasticManager = elasticManager ?? throw new ArgumentNullException(nameof(elasticManager));
            _ontologySchema = ontologySchema ?? throw new ArgumentNullException(nameof(ontologySchema));
            _extNodeService = extNodeService ?? throw new ArgumentNullException(nameof(extNodeService));
            _nodeRepository = nodeRepository ?? throw new ArgumentNullException(nameof(nodeRepository));
            this.ontologyNodesData = ontologyNodesData;
        }

        public bool IsIndexesValid(IEnumerable<string> indexes)
        {
            var notValidIndexes = indexes
                          .Where(name => !_elasticState.OntologyIndexes.Contains(name))
                          .ToList();

            if (notValidIndexes.Any())
            {
                TryLog($"There are not valid index names in list: {string.Join(", ", notValidIndexes)}");
                return false;
            }

            return true;
        }

        public async Task DeleteIndexesAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default)
        {
            var indexesToDelete = isHistorical ? GetHistoricalIndexes(indexes) : indexes;
            foreach (var index in indexesToDelete)
            {
                ct.ThrowIfCancellationRequested();
                var result = await _elasticManager.DeleteIndexAsync(index, ct);
                if (!result)
                    TryLog($"{index} was not deleted");
            }
        }

        public async Task CreateMappingsAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default)
        {
            foreach (var index in indexes)
            {
                ct.ThrowIfCancellationRequested();

                var historicalIndex = _elasticState.HistoricalOntologyIndexes[index];
                var attributesInfo = isHistorical
                    ? _ontologySchema.GetHistoricalAttributesInfo(index, historicalIndex)
                    : _ontologySchema.GetAttributesInfo(index);

                var result = await _elasticManager.CreateMapping(attributesInfo, ct);

                if (!result)
                    TryLog($"mapping was not created for {index}");
            }
        }

        public async Task FillIndexesAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default)
        {
            var nodeIds = await _extNodeService.GetExtNodesByTypeIdsAsync(indexes, ct);
            if (isHistorical)
            {
                var response = await _nodeRepository.PutHistoricalNodesAsync(nodeIds, ct);
                LogBulkResponse(response);
            }
            else
            {
                var results = new List<bool>(nodeIds.Count);
                foreach (var id in nodeIds)
                {
                    ct.ThrowIfCancellationRequested();

                    var result = await _nodeRepository.PutNodeAsync(id, ct);
                    if (!result)
                        TryLog($"node with id: {id} was not indexed");

                    results.Add(result);
                }

                TryLog($"nodes: {results.Count}");
                TryLog($"success indexed nodes: {results.Count(x => x)}");
                TryLog($"failed indexed nodes: {results.Count(x => !x)}");
            }
        }

        public async Task FillIndexesFromMemoryAsync(IEnumerable<string> indexes, bool isHistorical, CancellationToken ct = default)
        {
            var nodes = GetNodesFromMemory(indexes);
            var response = isHistorical
                ? await _nodeRepository.PutHistoricalNodesAsync(nodes, ct)
                : await _nodeRepository.PutNodesAsync(nodes, ct);

            LogBulkResponse(response);
        }


        private List<INode> GetNodesFromMemory(IEnumerable<string> indexes)
        {
            var nodes = new List<INode>();
            foreach (var index in indexes)
            {
                var type = _ontologySchema.GetEntityTypeByName(index);
                var entities = ontologyNodesData.GetEntitiesByTypeName(type.Name);
                nodes.AddRange(entities);
            }

            return nodes;
        }

        private void TryLog(string message)
        {
            if (Logger != null)
                Logger.AppendLine(message);

            return;
        }

        private void LogBulkResponse(IEnumerable<ElasticBulkResponse> response)
        {
            var successResponses = response.Where(x => x.IsSuccess);
            TryLog($"Success operations: {successResponses.Count()}");
            foreach (var item in successResponses.GroupBy(x => x.SuccessOperation))
            {
                TryLog($"nodes were {item.Key}: {item.Count()}");
            }

            var failedRespones = response.Where(x => !x.IsSuccess);
            TryLog($"Failed operations: {failedRespones.Count()}");
            foreach (var group in failedRespones.GroupBy(x => x.Id))
            {
                TryLog($"error occurred for Id:{group.Key}, errorType:{group.First().ErrorType}, error message:{group.First().ErrorReason}");
            }
        }

        private List<string> GetHistoricalIndexes(IEnumerable<string> indexes)
        {
            return indexes
                .Select(x => _elasticState.HistoricalOntologyIndexes.GetValueOrDefault(x))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
        }
    }
}