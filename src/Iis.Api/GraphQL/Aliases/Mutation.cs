using AutoMapper;
using HotChocolate;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using System.Threading.Tasks;

namespace Iis.Api.GraphQL.Aliases
{
    public class Mutation 
    {
        public async Task<Alias> CreateAlias(
           [Service] IAliasService aliasService,
           [Service] IMapper mapper,
           [GraphQLNonNullType] Alias data)
        {
            var aliasDto = mapper.Map<AliasDto>(data);

            var newAlias = await aliasService.CreateAsync(aliasDto);
            return mapper.Map<Alias>(newAlias);
        }
    }
}
