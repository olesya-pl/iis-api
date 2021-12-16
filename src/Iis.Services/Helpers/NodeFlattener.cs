using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.DbLayer.Repositories;
using Iis.Utility;

namespace Iis.Services.Helpers
{
    public class NodeFlattener<TUnitOfWork> : BaseService<TUnitOfWork>
        where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IElasticSerializer _elasticSerializer;
        private readonly IExtNodeService _extNodeService;
        private readonly IOntologyNodesData _ontologyData;

        public NodeFlattener(
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IElasticSerializer elasticSerializer,
            IExtNodeService extNodeService,
            IOntologyNodesData ontologyData)
        : base(unitOfWorkFactory)
        {
            _elasticSerializer = elasticSerializer;
            _extNodeService = extNodeService;
            _ontologyData = ontologyData;
        }

        internal async Task<FlattenNodeResult> FlattenNodeAsync(Guid id)
        {
            var node = _ontologyData.GetNode(id);
            var extNode = _extNodeService.GetExtNode(node);

            await PopulateExtNodeAsync(extNode);

            return new FlattenNodeResult
            {
                SerializedNode = _elasticSerializer.GetJsonByExtNode(extNode),
                Id = extNode.Id,
                NodeTypeName = extNode.NodeTypeName
            };
        }

        internal async Task<IReadOnlyCollection<FlattenNodeResult>> FlattenNodesAsync(IReadOnlyCollection<INode> itemsToUpdate)
        {
            var extNodeArray = await PopulateExtNodesAsync(_extNodeService.GetExtNodes(itemsToUpdate));

            return extNodeArray
                .Select(extNode => new FlattenNodeResult
                {
                    SerializedNode = _elasticSerializer.GetJsonByExtNode(extNode),
                    Id = extNode.Id,
                    NodeTypeName = extNode.NodeTypeName
                }).ToArray();
        }

        private async Task<IExtNode> PopulateExtNodeAsync(IExtNode extNode)
        {
            await AddSignLocationAsync(extNode);
            await PopulateChangeHistoryAsync(extNode);

            return extNode;
        }

        private async Task<IReadOnlyCollection<IExtNode>> PopulateExtNodesAsync(IReadOnlyCollection<IExtNode> extNodes)
        {
            await AddSignLocationsAsync(extNodes);
            await PopulateChangeHistoriesAsync(extNodes);

            return extNodes;
        }

        private async Task AddSignLocationAsync(IExtNode extNode)
        {
            var nodeType = _ontologyData.Schema.GetEntityTypeByName(extNode.EntityTypeName);

            if (!nodeType.IsObjectSign || !Guid.TryParse(extNode.Id, out Guid nodeId)) return;

            var latestLocation = await RunAsync(uow => uow.LocationHistoryRepository.GetLatestLocationHistoryEntityAsync(nodeId));

            if (latestLocation is null) return;

            extNode.Location = new GeoCoordinates(latestLocation.Lat, latestLocation.Long);
        }

        private async Task AddSignLocationsAsync(IReadOnlyCollection<IExtNode> extNodes)
        {
            var nodesToUpdate = new Dictionary<Guid, IExtNode>();

            foreach (var extNode in extNodes)
            {
                var nodeType = _ontologyData.Schema.GetEntityTypeByName(extNode.EntityTypeName);

                if (!nodeType.IsObjectSign || !Guid.TryParse(extNode.Id, out Guid nodeId)) continue;

                nodesToUpdate.Add(nodeId, extNode);
            }

            if (nodesToUpdate.Count == 0)
            {
                return;
            }

            var latestLocations = await RunAsync(uow => uow.LocationHistoryRepository.GetLatestLocationHistoryListAsync(nodesToUpdate.Keys));

            foreach (var latestLocation in latestLocations)
            {
                if (latestLocation.NodeId.HasValue && nodesToUpdate.ContainsKey(latestLocation.NodeId.Value))
                {
                    var extNode = nodesToUpdate[latestLocation.NodeId.Value];
                    extNode.Location = new GeoCoordinates(latestLocation.Lat, latestLocation.Long);
                }
            }
        }

        private async Task PopulateChangeHistoryAsync(IExtNode extNode)
        {
            if (!Guid.TryParse(extNode.Id, out Guid nodeId)) return;

            var changes = await RunWithoutCommitAsync(_ => _.ChangeHistoryRepository.GetByIdsAsync(nodeId.AsArray()));

            extNode.ChangeHistory = changes;
        }

        private async Task PopulateChangeHistoriesAsync(IReadOnlyCollection<IExtNode> extNodes)
        {
            var nodesToUpdate = new Dictionary<Guid, IExtNode>();

            if (nodesToUpdate.Count == 0)
            {
                return;
            }

            foreach (var extNode in extNodes)
            {
                if (!Guid.TryParse(extNode.Id, out Guid nodeId)) continue;

                nodesToUpdate.Add(nodeId, extNode);
            }

            var changes = await RunWithoutCommitAsync(_ => _.ChangeHistoryRepository.GetByIdsAsync(nodesToUpdate.Keys));

            foreach (var (nodeId, extNode) in nodesToUpdate)
            {
                extNode.ChangeHistory = changes.Where(p => p.TargetId == nodeId).ToArray();
            }
        }
    }

    public class FlattenNodeResult
    {
        public string SerializedNode { get; set; }
        public string Id { get; internal set; }
        public string NodeTypeName { get; internal set; }
    }
}