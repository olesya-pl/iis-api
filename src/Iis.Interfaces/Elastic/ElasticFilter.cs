using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public class ElasticFilter
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Suggestion { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public List<CherryPickedItem> CherryPickedItems { get; set; } = new List<CherryPickedItem>();
        public List<Property> FilteredItems { get; set; } = new List<Property>();
        public bool IsExact => IsExactQuery(Suggestion);

        private static bool IsExactQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return false;

            return query.Contains(":", StringComparison.Ordinal)
                   || query.Contains(" AND ", StringComparison.Ordinal)
                   || query.Contains(" OR ", StringComparison.Ordinal);
        }
    }

    public class CherryPickedItem
    {
        public string Item { get; set; }
        public bool IncludeDescendants { get; set; }

        public CherryPickedItem(string item, bool includeDescendants)
        {
            Item = item;
            IncludeDescendants = includeDescendants;
        }

        public CherryPickedItem(string item)
        {
            Item = item;
            IncludeDescendants = true;
        }
    }
}