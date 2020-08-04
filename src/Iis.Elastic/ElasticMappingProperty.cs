using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Elastic
{
    public class ElasticMappingProperty
    {
        public string Name { get; set; }
        public ElasticMappingPropertyType Type { get; set; }
        public List<string> Formats { get; } = new List<string>();
        public string Path { get; set; }
        public List<ElasticMappingProperty> Properties { get; set; } = new List<ElasticMappingProperty>();
        public bool SupportsNullValue { get; }

        public ElasticMappingProperty() { }

        public ElasticMappingProperty(string dotName, ElasticMappingPropertyType type, bool supportsNullValue = false, IEnumerable<string> formats = null)
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
            SupportsNullValue = supportsNullValue;

            if(formats != null && formats.Any())
            Formats.AddRange(formats);
        }

        public JObject ToJObject()
        {
            var result = new JObject();
            if (this.Type != ElasticMappingPropertyType.Nested)
            {
                result["type"] = this.Type.ToString().ToLower();
            }
            if (SupportsNullValue)
            {
                result["null_value"] = ElasticManager.NullValue;
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

        public override string ToString() => Name;
    }


}
