using System;
using System.Linq;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using Newtonsoft.Json.Linq;

namespace Iis.Services
{
    public class NodeToJObjectMapper
    {
        private readonly string[] EventDotNames = new[] {
            "name", "description", "updatedAt", "startsAt", "endsAt" };

        public JObject NodeToJObject(Node node)
        {
            if (node == null)
                return null;

            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            foreach (var attribute in node.GetChildAttributesExcludingNestedObjects().GroupBy(p => p.DotName))
            {
                if (attribute.Key == "attachment" || attribute.Key == "photo")
                {
                    result.Add(new JProperty(attribute.Key, attribute.Select(p =>
                    {
                        var res = new JObject();
                        res.Add("id", p.Attribute.Value.ToString());
                        res.Add("url", FileUrlGetter.GetFileUrl((Guid)p.Attribute.Value));
                        return res;
                    })));
                }
                else if (attribute.Count() == 1)
                {
                    result.Add(new JProperty(attribute.Key, attribute.FirstOrDefault().Attribute.Value.ToString()));
                }
                else
                {
                    result.Add(new JProperty(attribute.Key, attribute.Select(p => p.Attribute.Value.ToString()).ToList()));
                }
            }
            INodeTypeLinked nodeType = node.OriginalNode.NodeType;
            var titleRelationType = nodeType.GetRelationTypeByName("__title");
            if (titleRelationType != null)
            {
                var formula = titleRelationType.NodeType.MetaObject.Formula;
                if (!string.IsNullOrWhiteSpace(formula))
                {
                    result["__title"] = node.OriginalNode.ResolveFormula(formula);
                }
            }
            result.Add(new JProperty("__iconName", node.OriginalNode.NodeType.GetIconName()));

            return result;
        }

        public JObject EventToJObject(Node node)
        {
            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            var attributies = node.GetChildAttributesExcludingNestedObjects()
                .Where(a => EventDotNames.Contains(a.DotName));

            foreach (var attribute in attributies)
            {
                result.Add(new JProperty(attribute.DotName, attribute.Attribute.Value));
            }

            return result;
        }
    }
}
