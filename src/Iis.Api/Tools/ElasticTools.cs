using Iis.Domain.Elastic;
using IIS.Core.Ontology;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task RecreateElastic()
        {
            var nodeTypeIds = await _extNodeService.GetNodeTypesForElastic();
            var extNodes = await _extNodeService.GetExtNodesByTypeIds(nodeTypeIds);
            foreach (var extNode in extNodes)
            {
                await _elasticManager.InsertExtNodeAsync(extNode);
            }
        }
    }
}
