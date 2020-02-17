using Iis.Domain.ExtendedData;
using Iis.Interfaces.Ontology;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Elastic
{
    public class IisElasticSerializer
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
                json[nameof(extNode.CreatedAt)] = extNode.CreatedAt;
                json[nameof(extNode.UpdatedAt)] = extNode.UpdatedAt;
            }

            foreach (var child in extNode.Children)
            {
                if (child.IsAttribute)
                {
                    json[child.NodeTypeName] = child.AttributeValue;
                }
                else
                {
                    json[child.NodeTypeName] = GetJsonObjectByExtNode(child, false);
                }
            }

            return json;
        }
    }
}
