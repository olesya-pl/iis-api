using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Elastic
{
    public enum ElasticMappingPropertyType
    {
        Text,
        Integer,
        Date,
        Nested,
        Alias
    }
    public class ElasticMappingProperty
    {
        public string Name { get; set; }
        public ElasticMappingPropertyType Type { get; set; }
        public string Path { get; set; }
        public List<ElasticMappingProperty> Properties { get; set; } = new List<ElasticMappingProperty>();

        public JObject ConvertToJObject()
        {
            var inner = new JObject();
            if (this.Type != ElasticMappingPropertyType.Nested)
            {
                inner["type"] = this.Type.ToString().ToLower();
            }

            if (Properties.Count > 0)
            {
                var jProperties = new JObject();
                foreach (var property in Properties)
                {
                    jProperties[property.Name] = property.ConvertToJObject();
                }
                inner["properties"] = jProperties;
            }
            return inner;
        }
    }

    
}
