using Iis.Interfaces.Enums;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Elastic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Services.Elastic
{
    public class MaterialElasticResponseManager : IElasticResponseManager
    {
        private readonly IAliasService _aliasService;
        private Dictionary<string, AliasDto> _aliasesByName;
        public MaterialElasticResponseManager(IAliasService aliasService)
        {
            _aliasService = aliasService;
            _aliasesByName = new Dictionary<string, AliasDto>();
        }

        public async Task<JObject> GenerateHighlightsWithoutDublications(JObject source, JToken highlights)
        {
            var aliasesByName = await GetAliasesByNameAsync();
            var result = new JObject();
            foreach (JProperty child in highlights.Children())
            {
                if (!aliasesByName.ContainsKey(child.Name))
                {
                    result[child.Name] = child.Value;
                }
            }

            return result;
        }

        private async Task<Dictionary<string, AliasDto>> GetAliasesByNameAsync() 
        {
            if (!_aliasesByName.Any())
                _aliasesByName = (await _aliasService.GetByTypeAsync(AliasType.Material)).ToDictionary(x => x.DotName);

            return _aliasesByName;
        }
    }
}
