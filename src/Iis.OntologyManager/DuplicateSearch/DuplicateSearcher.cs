using Iis.Interfaces.Ontology.Data;
using Iis.OntologyData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyManager.DuplicateSearch
{
    public class DuplicateSearcher
    {
        OntologyNodesData _data;

        public DuplicateSearcher(OntologyNodesData data)
        {
            _data = data;
        }
        private string GetUrl(string baseUrl, INode node)
        {
            return $"{baseUrl}/objects/Entity{node.NodeType.Name}.{node.Id}/view";
        }
        public DuplicateSearchResult Search(DuplicateSearchParameter param)
        {
            var allValues = GetAllValues(param);
            var result = new DuplicateSearchResult();
            DuplicateSearchResultItem previousItem = null;
            int orderNumber = 0;
            bool isNew = false;

            foreach (var item in allValues)
            {
                if (item.Value == previousItem?.Value && item.Node.HasTheSameValues(previousItem?.Node, param.DistinctNames))
                {
                    if (isNew)
                    {
                        orderNumber++;
                        result.Items.Add(previousItem);
                        previousItem.OrderNumber = orderNumber;
                        previousItem.Url = GetUrl(param.Url, previousItem.Node);
                    }

                    result.Items.Add(item);
                    item.OrderNumber = orderNumber;
                    item.Url = GetUrl(param.Url, item.Node);
                    isNew = false;
                } else {
                    isNew = true;
                }
                previousItem = item;
            }
            return result;
        }
        private List<DuplicateSearchResultItem> GetAllValues(DuplicateSearchParameter param)
        {
            var list = new List<DuplicateSearchResultItem>();

            var entities = _data.GetEntitiesByTypeName(param.EntityTypeName);
            foreach (var entity in entities)
            {
                var addedValues = new HashSet<string>();

                var dotNameValues = entity.GetDotNameValues();
                foreach (var dotName in param.DotNames)
                {
                    var value = entity.GetSingleProperty(dotName)?.Value;
                    if (!string.IsNullOrEmpty(value) && !addedValues.Contains(value))
                    {
                        list.Add(new DuplicateSearchResultItem
                        {
                            Value = value,
                            DotName = dotName,
                            Node = entity
                        });
                        addedValues.Add(value);
                    }
                }
            }

            var result = list.OrderBy(t => t.Value);
            foreach (var distinctName in param.DistinctNames)
            {
                result = result.ThenBy(t => t.Node.GetSingleProperty(distinctName)?.Value);
            }
            return result.ToList();
        }
    }
}
