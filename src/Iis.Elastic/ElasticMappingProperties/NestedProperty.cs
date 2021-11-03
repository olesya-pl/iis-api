using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class NestedProperty : ElasticMappingProperty
    {
        private NestedProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Nested;

        public static ElasticMappingProperty Create(string name, List<ElasticMappingProperty> properties)
        {
            return new NestedProperty
            {
                Name = name,
                Properties = properties
            };
        }

        public static ElasticMappingProperty Create(string propertyName, ElasticMappingProperty nestedProperty)
        {
            var result = new NestedProperty { Name = propertyName };

            if (nestedProperty != null)
            {
                result.Properties.Add(nestedProperty);
            }

            return result;
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (Properties.Count == 0) return;

            var jProperties = new JObject();
            foreach (var property in Properties)
            {
                jProperties[property.Name] = property.ToJObject();
            }
            result["properties"] = jProperties;
        }
    }
}
