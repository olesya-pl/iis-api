using System;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories;
using Newtonsoft.Json;

namespace Iis.Api.Export
{
    public class ExportToJsonService
    {
        private readonly INodeRepository _nodeRepository;

        public ExportToJsonService(
            INodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
        }

        public async Task<string> ExportNodeAsync(Guid id)
        {
            return (await _nodeRepository.GetJsonNodeByIdAsync(id))?.ToString(Formatting.Indented);
        }
    }
}
