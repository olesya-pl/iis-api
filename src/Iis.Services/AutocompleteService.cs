﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain.Users;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Newtonsoft.Json.Linq;

namespace Iis.Services
{
    public class AutocompleteService : IAutocompleteService
    {
        private static readonly IReadOnlyCollection<AutocompleteEntityDto> EmptyAutoCompleteList = Array.Empty<AutocompleteEntityDto>();
        private static readonly List<string> KeyWords = new List<string>();
        private static readonly string[] SearchableFields = { "__title", "commonInfo.RealNameShort", "title" };
        private readonly IOntologySchema _ontologySchema;
        private readonly IElasticService _elasticService;
        private readonly IElasticManager _elasticManager;

        public AutocompleteService(IOntologySchema ontologySchema, IElasticService elasticService, IElasticManager elasticManager)
        {
            _ontologySchema = ontologySchema;
            _elasticService = elasticService;
            _elasticManager = elasticManager;
        }

        public IReadOnlyCollection<string> GetTips(string query, int count)
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
                .ToArray();
        }

        public async Task<IReadOnlyCollection<AutocompleteEntityDto>> GetEntitiesAsync(string query, string[] types, int size, User user, CancellationToken ct = default)
        {
            if (SearchQueryExtension.IsMatchAll(query)) return EmptyAutoCompleteList;

            var typeNameList = _ontologySchema.GetEntityTypesByName(types, includeChildren: true)
                                .Select(e => e.Name)
                                .Distinct()
                                .ToArray();

            if (!_elasticService.TypesAreSupported(typeNameList)) return EmptyAutoCompleteList;

            var autocompleteQuery = new DisjunctionQueryBuilder(query, SearchableFields)
                .WithPagination(0, size)
                .BuildSearchQuery()
                .ToString();

            var response = await _elasticManager.SearchAsync(autocompleteQuery, typeNameList, ct);

            return response.Items.Select(x => new AutocompleteEntityDto
            {
                Id = x.Identifier,
                Title = GetFirstNotNullField(x.SearchResult),
                TypeName = x.SearchResult["NodeTypeName"].Value<string>(),
                TypeTitle = x.SearchResult["NodeTypeTitle"].Value<string>()
            }).ToArray();
        }

        private static string GetFirstNotNullField(JObject jObject)
        {
            string result = null;
            foreach (var item in SearchableFields)
            {
                try
                {
                    var value = jObject[item];
                    if (value is JArray)
                    {
                        result = value.FirstOrDefault(item => !string.IsNullOrEmpty(item.ToString())).ToString();
                        if (!string.IsNullOrEmpty(result))
                            return result;
                    }
                    else
                    {
                        result = jObject.SelectToken(item)?.Value<string>();
                        if (!string.IsNullOrEmpty(result))
                            return result;
                    }
                }
                catch (InvalidCastException)
                {
                    continue;
                }
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
                    .Select(x => x.Key.Substring(x.Key.IndexOf('.', StringComparison.OrdinalIgnoreCase) + 1))
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
