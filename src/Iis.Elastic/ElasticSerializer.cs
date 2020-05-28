using Newtonsoft.Json.Linq;
using System.Linq;

using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Schema;
using System;

namespace Iis.Elastic
{
    public class ElasticSerializer : IElasticSerializer
    {
        public string GetJsonByExtNode(IExtNode extNode)
        {
            return GetJsonObjectByExtNode(extNode).ToString();
        }
        public JObject GetJsonObjectByExtNode(IExtNode extNode, bool IsHeadNode = true)
        {
            var json = new JObject();

            if (IsHeadNode)
            {
                json[nameof(extNode.Id)] = extNode.Id;
                json[nameof(extNode.NodeTypeName)] = extNode.NodeTypeName;
                if (!string.IsNullOrEmpty(extNode.NodeTypeTitle))
                {
                    json[nameof(extNode.NodeTypeTitle)] = extNode.NodeTypeTitle;
                }
                json[nameof(extNode.CreatedAt)] = extNode.CreatedAt;
                json[nameof(extNode.UpdatedAt)] = extNode.UpdatedAt;
            }

            foreach (var childGroup in extNode.Children.GroupBy(p => p.NodeTypeName))
            {
                var key = childGroup.Key;
                if (childGroup.Count() == 1)
                {
                    var child = childGroup.First();
                    json[key] = GetExtNodeValue(child);
                }
                else
                {
                    var items = new JArray();
                    json[key] = items;
                    foreach (var child in childGroup)
                    {
                        items.Add(GetExtNodeValue(child));
                    }
                }
            }

            return json;
        }

        private JToken GetFuzzyDateJObject(IExtNode extNode)
        {
            int? year = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "year")?.AttributeValue;
            int? month = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "month")?.AttributeValue;
            int? day = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "day")?.AttributeValue;

            var date = new DateTime((int)year, (int)month, (int)day);
            return JToken.FromObject(date);
        }

        private JToken GetExtNodeValue(IExtNode extNode)
        {
            if (extNode.EntityTypeName == EntityTypeNames.FuzzyDate.ToString())
            {
                return GetFuzzyDateJObject(extNode);
            }

            if (extNode.IsAttribute && extNode.AttributeValue != null)
            {
                return JToken.FromObject(extNode.AttributeValue);
            }
            
            return GetJsonObjectByExtNode(extNode, false);
        }
    }
}
