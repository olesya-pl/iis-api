using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace IIS.Core.GraphQL.Entities
{
    public class AggregationNodeTypeTree
    {
        private const string EntityName = "Entity";

        private List<AggregationNodeTypeItem> _items;
        private Dictionary<string, AggregationNodeTypeItem> _itemsDict;

        public AggregationNodeTypeTree(IEnumerable<AggregationNodeTypeItem> items)
        {
            _items = items.ToList();
            _itemsDict = new Dictionary<string, AggregationNodeTypeItem>();
            AddItems(items);
        }

        public IReadOnlyList<AggregationNodeTypeItem> Items => _items;

        public void MergeBuckets(IReadOnlyList<AggregationBucket> buckets, ILogger logger = null)
        {
            if (buckets == null || buckets.Count == 0)
            {
                _items = new List<AggregationNodeTypeItem>();
                _itemsDict = new Dictionary<string, AggregationNodeTypeItem>();
                return;
            }

            foreach (var bucket in buckets)
            {
                var typeName = bucket.TypeName?.StartsWith(EntityName) ?? false
                    ? bucket.TypeName.Substring(EntityName.Length)
                    : bucket.TypeName;
                if (string.IsNullOrEmpty(typeName))
                {
                    logger?.LogError("MergeBuckets: typeName is empty");
                    continue;
                }

                var item = _itemsDict.GetValueOrDefault(typeName);
                if (item != null)
                {
                    item.DocCount = bucket.DocCount;
                    continue;
                }

                logger?.LogError(GetItemNotFoundMessage(typeName));
            }

            SummarizeCounts(_items);
            RemoveZeroes(_items);
        }

        private void AddItems(IEnumerable<AggregationNodeTypeItem> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                _itemsDict[item.NodeTypeName] = item;
                AddItems(item.Children);
            }
        }

        private void SummarizeCounts(IReadOnlyList<AggregationNodeTypeItem> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                SummarizeCounts(item.Children);
                item.DocCount += item.Children?.Sum(_ => _.DocCount) ?? 0;
            }
        }

        private void RemoveZeroes(List<AggregationNodeTypeItem> items)
        {
            if (items == null) return;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item.DocCount == 0)
                {
                    items.RemoveAt(i);
                }
                else
                {
                    RemoveZeroes(item.Children);
                }
            }
        }

        private string GetItemNotFoundMessage(string typeName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"AggregationNodeTypeTree.MergeBuckets error: '{typeName}' is not found in _itemsTree");
            sb.AppendLine("_itemsDict keys: ");
            sb.Append(string.Join(", ", _itemsDict.Keys.OrderBy(_ => _)));
            return sb.ToString();
        }
    }
}