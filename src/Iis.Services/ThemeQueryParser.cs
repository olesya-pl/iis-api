using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.Services
{
    internal class ThemeQueryParser
    {
        public static ThemeQuery Parse(string queryRequest)
        {
            try
            {
                var queryResult = JObject.Parse(queryRequest);
                if (queryRequest == null)
                {
                    return null;
                }
                var suggestion = string.Empty;
                if (queryResult.ContainsKey("suggestion"))
                {
                    suggestion = queryResult["suggestion"].Value<string>();
                }

                var cherryPickedItems = Enumerable.Empty<CherryPickedItem>().ToList();
                if (queryResult.ContainsKey("selectedEntities"))
                {
                    cherryPickedItems = queryResult.SelectToken("selectedEntities", false)
                        .AsEnumerable()
                        .Select(p => new CherryPickedItem(p.Value<string>("id"), p.Value<bool>("includeDescendants")))
                        .ToList();
                }

                SearchByImageInput searchByImageInput = null;
                if (queryResult.ContainsKey("searchByImageInput"))
                {
                    var searchByImageInputJson = queryResult["searchByImageInput"] as JObject;

                    if (searchByImageInputJson != null)
                    {
                        searchByImageInput = searchByImageInputJson.ToObject<SearchByImageInput>();
                    }
                }

                SearchByRelationInput searchByRelation = null;
                if (queryResult.ContainsKey("searchByRelation"))
                {
                    var searchByRelationInputJson = queryResult["searchByRelation"] as JObject;

                    if (searchByRelationInputJson != null)
                    {
                        searchByRelation = searchByRelationInputJson.ToObject<SearchByRelationInput>();
                    }
                }

                var filteredItems = Array.Empty<FilteredItem>();

                if (queryResult.ContainsKey("filteredItems"))
                {
                    var filteredItemsJson = queryResult["filteredItems"] as JArray;
                    if (filteredItemsJson != null)
                    {
                        filteredItems = filteredItemsJson.ToObject<FilteredItem[]>();
                    }
                }

                return new ThemeQuery
                {
                    Suggestion = suggestion,
                    CherryPickedItems = cherryPickedItems,
                    SearchByImageInput = searchByImageInput,
                    SearchByRelation = searchByRelation,
                    FilteredItems = filteredItems
                        .Select(item =>
                        {
                            var properties = new List<Property>();
                            foreach (var value in item.Value)
                            {
                                properties.Add(new Property()
                                {
                                    Name = item.Name,
                                    Value = value
                                });
                            }
                            return properties;
                        })
                        .SelectMany(p => p)
                        .ToList()
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

    internal class ThemeQuery
    {
        public string Suggestion { get; set; }
        public IReadOnlyCollection<CherryPickedItem> CherryPickedItems { get; set; } = new List<CherryPickedItem>();
        public IReadOnlyCollection<Property> FilteredItems { get; set; } = new List<Property>();
        public SearchByImageInput SearchByImageInput { get; set; }
        public SearchByRelationInput SearchByRelation { get; set; }
    }

    internal class SearchByImageInput
    {
        public string Name { get; set; }
        public string Content { get; set; }
        [JsonIgnore]
        public bool HasConditions => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Content);
    }

    internal class SearchByRelationInput
    {
        public IEnumerable<Guid> NodeIdentityList { get; set; }
        public bool IncludeDescendants { get; set; }
        [JsonIgnore]
        public bool HasConditions => NodeIdentityList != null && NodeIdentityList.Any();
    }

    internal class FilteredItem
    {
        public string Name { get; set; }
        public string[] Value { get; set; }
    }
}
