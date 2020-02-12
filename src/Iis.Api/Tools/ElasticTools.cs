﻿using Iis.Domain.Elastic;
using IIS.Core.Ontology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Tools
{
    public class ElasticTools
    {
        IExtNodeService _extNodeService;
        IElasticManager _elasticManager;
        public ElasticTools(IExtNodeService extNodeService, IElasticManager elasticManager)
        {
            _extNodeService = extNodeService;
            _elasticManager = elasticManager;
        }

        public async Task RecreateElasticAsync(CancellationToken cancellationToken = default)
        {
            var nodeTypes = await _extNodeService.GetNodeTypesForElasticAsync(cancellationToken);
            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(nodeTypes.Select(nt => nt.Id), cancellationToken);
            _elasticManager.SetSupportedIndexes(nodeTypes.Select(nt => nt.Name));
            foreach (var extNode in extNodes)
            {
                await _elasticManager.PutExtNodeAsync(extNode);
            }
        }
    }
}
