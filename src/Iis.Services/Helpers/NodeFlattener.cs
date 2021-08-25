﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.DbLayer.Repositories;

namespace Iis.Services.Helpers
{
    public class NodeFlattener<TUnitOfWork> : BaseService<TUnitOfWork> where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IElasticSerializer _elasticSerializer;
        private readonly IExtNodeService _extNodeService;
        private readonly IOntologyNodesData _ontologyData;

        public NodeFlattener(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
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

            extNode = await AddSingLocationAsync(extNode);

            return new FlattenNodeResult
            {
                SerializedNode = _elasticSerializer.GetJsonByExtNode(extNode),
                Id = extNode.Id,
                NodeTypeName = extNode.NodeTypeName
            };
        }

        internal async Task<IReadOnlyCollection<FlattenNodeResult>> FlattenNodesAsync(IReadOnlyCollection<INode> itemsToUpdate)
        {
            var extNodeTaskList = _extNodeService.GetExtNodes(itemsToUpdate)
                            .Select(extNode => AddSingLocationAsync(extNode));

            var extNodeArray = await Task.WhenAll(extNodeTaskList);

            return extNodeArray
                .Select(extNode => new FlattenNodeResult
                {
                    SerializedNode = _elasticSerializer.GetJsonByExtNode(extNode),
                    Id = extNode.Id,
                    NodeTypeName = extNode.NodeTypeName
                }).ToArray();
        }

        private async Task<IExtNode> AddSingLocationAsync(IExtNode extNode)
        {
            var nodeType = _ontologyData.Schema.GetEntityTypeByName(extNode.EntityTypeName);

            if(!nodeType.IsObjectSign || !Guid.TryParse(extNode.Id, out Guid nodeId)) return extNode;

            var latestLocation = await RunAsync(uow => uow.LocationHistoryRepository.GetLatestLocationHistoryEntityAsync(nodeId));

            if(latestLocation is null) return extNode;

            extNode.Location = new GeoCoordinates(latestLocation.Lat, latestLocation.Long);

            return extNode;
        }
    }

    public class FlattenNodeResult
    {
        public string SerializedNode { get; set; }
        public string Id { get; internal set; }
        public string NodeTypeName { get; internal set; }
    }
}