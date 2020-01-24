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
        public ElasticTools(IExtNodeService extNodeService)
        {
            _extNodeService = extNodeService;
        }

        public async Task RecreateElastic()
        {
            var nodeTypeIds = await _extNodeService.GetNodeTypesForElastic();
            var extNodes = await _extNodeService.GetExtNodesByTypeIds(nodeTypeIds);
            
        }
    }
}
