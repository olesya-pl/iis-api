using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Elastic
{
    public enum ElasticMappingPropertyType : byte
    {
        Text,
        Integer,
        Date,
        Nested,
        Alias,
        Keyword
    }
    public class ElasticMappingProperty
    {
        public ElasticMappingProperty() { }
        public ElasticMappingProperty(string dotName, ElasticMappingPropertyType type)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            Name = splitted[0];
            if (splitted.Length == 1)
            {
                Type = type;
            }
            else
            {
                Type = ElasticMappingPropertyType.Nested;
                Properties = new List<ElasticMappingProperty>
                {
                    new ElasticMappingProperty(string.Join('.', splitted.Skip(1)), type)
                };
            }
        }

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
