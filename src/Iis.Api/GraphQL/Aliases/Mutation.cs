using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using System;
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

        public async Task<Alias> UpdateAlias(
           [Service] IAliasService aliasService,
           [Service] IMapper mapper,
           [GraphQLNonNullType] Alias data)
        {
            var aliasDto = mapper.Map<AliasDto>(data);

            var newAlias = await aliasService.UpdateAsync(aliasDto);
            return mapper.Map<Alias>(newAlias);
        }

        public async Task<Alias> RemoveAlias(
           [Service] IAliasService aliasService,
           [Service] IMapper mapper,
           [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var removedAlias = await aliasService.RemoveAsync(id);
            return mapper.Map<Alias>(removedAlias);
        }
    }
}
