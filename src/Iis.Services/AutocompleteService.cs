using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services
{
    public class AutocompleteService : IAutocompleteService
    {
        private readonly IOntologySchema _ontologySchema;
        private readonly IElasticService _elasticService;
        private const int DefaultSize = 10;
        private static readonly List<string> KeyWords = new List<string>();
        private static readonly string[] SearchableFields = new string[] { "__title" };

        public AutocompleteService(IOntologySchema ontologySchema, IElasticService elasticService)
        {
            _ontologySchema = ontologySchema;
            _elasticService = elasticService;
        }

        public List<string> GetTips(string query, int count)
        {
            var result = new List<string>(count);
            result.AddRange(GetKeyWords().Where(x => x.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)));

            if (result.Count < count)
            {
                result.AddRange(GetKeyWords().Where(x => x.Contains(query, StringComparison.InvariantCultureIgnoreCase)));
            }

            return result
                .Distinct()
                .Take(count)
                .ToList();
        }

        public async Task<List<AutocompleteEntityDto>> GetEntitiesAsync(string query, int? size, CancellationToken ct = default)
        {
            if(query.Trim().Equals("*")) return new List<AutocompleteEntityDto>();

            if (!query.Contains('*')) query = $"*{query}*";

            var response = await _elasticService.SearchByFieldsAsync(query, SearchableFields, size.GetValueOrDefault(DefaultSize), ct);

            return response.Select(x => new AutocompleteEntityDto
            {
                Id = x.Identifier,
                Title = GetFirstNotNullField(x.SearchResult),
                TypeName = x.SearchResult["NodeTypeName"].Value<string>(),
                TypeTitle = x.SearchResult["NodeTypeTitle"].Value<string>()
            }).ToList();
        }

        private string GetFirstNotNullField(JObject jObject) 
        {
            foreach (var item in SearchableFields)
            {
                var result = jObject.SelectToken(item)?.Value<string>();
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            return null;
        }

        private List<string> GetKeyWords()
        {
            if (!KeyWords.Any())
            {
                var aliases = _ontologySchema.Aliases.Items.Select(x => x.Value).ToList();
                var fields = _ontologySchema.GetFullHierarchyNodes()
                    .Where(x => IsEligibleNodeType(x.Value))
                    .Select(x => x.Key.Substring(x.Key.IndexOf('.') + 1))
                    .Distinct()
                    .ToList();

                KeyWords.AddRange(aliases);
                KeyWords.AddRange(fields);
            }

            return KeyWords;
        }

        private bool IsEligibleNodeType(INodeTypeLinked nodeType)
        {
            if (nodeType.Kind == Kind.Attribute && !_ontologySchema.IsFuzzyDateEntityAttribute(nodeType)) return true;

            if (_ontologySchema.IsFuzzyDateEntity(nodeType)) return true;

            return false;
        }
    }
}
