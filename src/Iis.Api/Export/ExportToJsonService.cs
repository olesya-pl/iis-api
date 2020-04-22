using System;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;

namespace Iis.Api.Export
{
    public class ExportToJsonService
    {
        private readonly IExtNodeService _nodeService;
        private readonly IElasticSerializer _elasticSerializer;

        public ExportToJsonService(
            IExtNodeService nodeService,
            IElasticSerializer elasticSerializer)
        {
            _nodeService = nodeService;
            _elasticSerializer = elasticSerializer;
        }

        public async Task<string> ExportNodeAsync(Guid id)
        {
            var extNode = await _nodeService.GetExtNodeByIdAsync(id);
            return _elasticSerializer.GetJsonByExtNode(extNode);
        }
    }
}
