using HotChocolate;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Autocomplete
{
    public class Query
    {
        private const int DefaultTipsCount = 20;

        public List<string> GetTips(
            [Service] IAutocompleteService autocomplete,
            [GraphQLNonNullType] string query, 
            int? count)
        {
            return autocomplete.GetTips(query, count.GetValueOrDefault(DefaultTipsCount));
        }

        public Task<List<AutocompleteEntityDto>> GetEntities(
            [Service] IAutocompleteService autocomplete,
            [GraphQLNonNullType] string query,
            int? size)
        {
            return autocomplete.GetEntitiesAsync(query, size);
        }
    }
}
