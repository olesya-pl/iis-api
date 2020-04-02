using Newtonsoft.Json.Linq;

using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
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

            foreach (var child in extNode.Children)
            {
                var key = GetUniqueKey(json, child.NodeTypeName);
                if (child.IsAttribute)
                {
                    json[key] = child.AttributeValue;
                }
                else
                {
                    if (child.Children.Count == 1 && child.Children[0].NodeTypeName == "value")
                    {
                        json[key] = child.Children[0].AttributeValue;
                    }
                    else
                    {
                        json[key] = GetJsonObjectByExtNode(child, false);
                    }
                }
            }

            return json;
        }
        private string GetUniqueKey(JObject json, string baseKey)
        {
            if (!json.ContainsKey(baseKey)) return baseKey;
            var n = 1;
            string key;
            do
            {
                key = $"{baseKey}{n++}";
            }
            while (json.ContainsKey(key));
            return key;
        }

    }
}
