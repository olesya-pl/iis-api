using AutoMapper;
using HotChocolate;
using Iis.Interfaces.Enums;
using Iis.Services.Contracts.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.Aliases
{
    public class Query
    {
        public async Task<List<Alias>> GetAliases(
            [Service] IAliasService aliasService,
            [Service] IMapper mapper,
            AliasType? type)
        {
            var aliases = type.HasValue
                ? await aliasService.GetByTypeAsync(type.Value)
                : await aliasService.GetAllAsync();
            return mapper.Map<List<Alias>>(aliases);
        }
    }
}
