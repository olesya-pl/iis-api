using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Entities
{
    public class AggregationNodeTypeTree
    {
        private List<AggregationNodeTypeItem> _items;
        private Dictionary<string, AggregationNodeTypeItem> _itemsDict;
        public IReadOnlyList<AggregationNodeTypeItem> Items => _items;
        public AggregationNodeTypeTree(IEnumerable<AggregationNodeTypeItem> items)
        {
            _items = items.ToList();
            _itemsDict = new Dictionary<string, AggregationNodeTypeItem>();
            AddItems(items);
        }
        public void MergeBuckets(IReadOnlyList<AggregationBucket> buckets)
        {
            if (buckets == null || buckets.Count == 0)
            {
                _items = new List<AggregationNodeTypeItem>();
                _itemsDict = new Dictionary<string, AggregationNodeTypeItem>();
                return;
            }

            foreach (var bucket in buckets)
            {
                var typeName = bucket.TypeName.StartsWith("Entity") ?
                    bucket.TypeName.Substring("Entity".Length) :
                    bucket.TypeName;

                var item = _itemsDict.GetValueOrDefault(typeName);
                item.DocCount = bucket.DocCount;
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
    }
}
