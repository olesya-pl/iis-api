using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Autocomplete
{
    public class Query
    {
        private const int DefaultTipsCount = 20;
        private static readonly string[] ObjectTypeList = new[] { EntityTypeNames.Object.ToString()};

        public IReadOnlyCollection<string> GetTips(
            [Service] IAutocompleteService autocomplete,
            [GraphQLNonNullType] string query, 
            int? count)
        {
            return autocomplete.GetTips(query, count.GetValueOrDefault(DefaultTipsCount));
        }

        public Task<IReadOnlyCollection<AutocompleteEntityDto>> GetEntities(
            IResolverContext context,
            [Service] IAutocompleteService autocomplete,
            [GraphQLNonNullType] string query,
            [GraphQLType(typeof(ListType<NonNullType<StringType>>))] string[] types,
            int? size)
        {
            types = types is null || !types.Any() ? ObjectTypeList : types;

            var token = context.GetToken();

            return autocomplete.GetEntitiesAsync(query, types, size.GetValueOrDefault(DefaultTipsCount), token.User);
        }
    }
}
