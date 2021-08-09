using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Iis.Services.Contracts.Interfaces;
namespace Iis.Api.Export
{
    public class ExportToJsonService
    {
        private readonly INodeSaveService _nodeRepository;

        public ExportToJsonService(
            INodeSaveService nodeRepository)
        {
            _nodeRepository = nodeRepository;
        }

        public async Task<string> ExportNodeAsync(Guid id)
        {
            return (await _nodeRepository.GetJsonNodeByIdAsync(id))?.ToString(Formatting.Indented);
        }
    }
}
