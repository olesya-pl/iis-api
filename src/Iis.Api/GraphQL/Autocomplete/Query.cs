using HotChocolate;
using Iis.Services;
using System.Collections.Generic;

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
    }
}
