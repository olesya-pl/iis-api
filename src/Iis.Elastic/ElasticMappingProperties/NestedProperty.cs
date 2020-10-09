using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class NestedProperty : ElasticMappingProperty
    {
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Nested;

        private NestedProperty() { }

        public static ElasticMappingProperty Create(string name, List<ElasticMappingProperty> properties)
        {
            return new NestedProperty {
                Name = name,
                Properties = properties
            };
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (Properties.Count > 0)
            {
                var jProperties = new JObject();
                foreach (var property in Properties)
                {
                    jProperties[property.Name] = property.ToJObject();
                }
                result["properties"] = jProperties;
            }
        }        
    }
}
