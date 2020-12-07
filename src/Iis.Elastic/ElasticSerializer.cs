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

            if(extNode.NodeType.IsObjectOfStudy)
            {
                json[nameof(extNode.Id)] = extNode.Id;
            }

            if (IsHeadNode)
            {
                json[nameof(extNode.NodeTypeName)] = extNode.NodeTypeName;
                if (!string.IsNullOrEmpty(extNode.NodeTypeTitle))
                {
                    json[nameof(extNode.NodeTypeTitle)] = extNode.NodeTypeTitle;
                    json[$"{nameof(extNode.NodeTypeTitle)}{ElasticManager.AggregateSuffix}"] = extNode.NodeTypeTitle;
                }
                json[nameof(extNode.CreatedAt)] = extNode.CreatedAt;
                json[nameof(extNode.UpdatedAt)] = extNode.UpdatedAt;
            } 

            var coordinates = extNode.GetCoordinatesWithoutNestedObjects();
            if (coordinates != null && coordinates.Count > 0)
            {
                var coords = new JArray();
                foreach (var coord in coordinates)
                {
                    var item = new JObject();
                    item["lat"] = coord.Latitude;
                    item["long"] = coord.Longitude;
                    coords.Add(item);
                }
                json["__coordinates"] = coords;
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

        private JToken GetFuzzyDateJToken(IExtNode extNode)
        {
            int? year = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "year")?.AttributeValue;
            if (year == null)
            {
                return null;
            }
            int month = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "month")?.AttributeValue ?? 1;
            int day = (int?)extNode.Children.SingleOrDefault(c => c.NodeTypeName == "day")?.AttributeValue ?? 1;

            try
            {
                var date = new DateTime((int)year, (int)month, (int)day);
                return JToken.FromObject(date);
            }
            catch
            {
                return null;
            }
        }

        private JToken GetExtNodeValue(IExtNode extNode)
        {
            if (extNode.EntityTypeName == EntityTypeNames.FuzzyDate.ToString())
            {
                return GetFuzzyDateJToken(extNode);
            }

            if (extNode.IsAttribute && extNode.AttributeValue != null)
            {
                return JToken.FromObject(extNode.AttributeValue);
            }

            return GetJsonObjectByExtNode(extNode, false);
        }
    }
}
