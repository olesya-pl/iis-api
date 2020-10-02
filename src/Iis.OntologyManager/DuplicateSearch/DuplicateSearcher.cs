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
                if (item.Value == previousItem?.Value)
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
                    previousItem = item;
                    isNew = true;
                }
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
                    var values = dotNameValues.GetItems(dotName);
                    foreach (var value in values)
                    {
                        if (!addedValues.Contains(value.Value))
                        {
                            list.Add(new DuplicateSearchResultItem
                            {
                                Value = value.Value,
                                DotName = dotName,
                                Node = entity
                            });
                            addedValues.Add(value.Value);
                        }
                    }
                }
            }

            return list.OrderBy(t => t.Value).ToList();
        }
    }
}
