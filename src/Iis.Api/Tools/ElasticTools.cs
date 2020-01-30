using Iis.Domain.Elastic;
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
            var nodeTypeIds = await _extNodeService.GetNodeTypesForElasticAsync(cancellationToken);
            var extNodes = await _extNodeService.GetExtNodesByTypeIdsAsync(nodeTypeIds, cancellationToken);
            foreach (var extNode in extNodes)
            {
                await _elasticManager.PutExtNodeAsync(extNode);
            }
        }
    }
}
