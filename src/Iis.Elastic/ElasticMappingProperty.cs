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

        public JObject ToJObject()
        {
            var result = new JObject();
            if (this.Type != ElasticMappingPropertyType.Nested)
            {
                result["type"] = this.Type.ToString().ToLower();
            }

            if (this.Type == ElasticMappingPropertyType.Alias)
            {
                result["path"] = this.Path;
            }

            if (Properties.Count > 0)
            {
                var jProperties = new JObject();
                foreach (var property in Properties)
                {
                    jProperties[property.Name] = property.ToJObject();
                }
                result["properties"] = jProperties;
            }
            return result;
        }
    }

    
}
