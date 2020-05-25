using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Elastic
{
    public class ElasticMappingConfiguration
    {
        public List<ElasticMappingProperty> Properties { get; } = new List<ElasticMappingProperty>();

        public ElasticMappingConfiguration() { }
        public ElasticMappingConfiguration(IAttributeInfoList attributeInfo)
        {
            foreach (var item in attributeInfo.Items)
            {
                var nameParts = item.DotName.Split('.');
                var mappingType = ToMappingType(item.ScalarType);
                AddProperty(Properties, nameParts, mappingType);
            }
        }

        public JObject ConvertToJObject()
        {
            var jProperties = new JObject();
            foreach (var property in Properties)
            {
                jProperties[property.Name] = property.ConvertToJObject();
            }
            var inner = new JObject();
            inner["properties"] = jProperties;
            var result = new JObject();
            result["mappings"] = inner;
            return result;
        }

        public ElasticMappingPropertyType ToMappingType(ScalarType scalarType)
        {
            switch (scalarType)
            {
                case ScalarType.Int:
                    return ElasticMappingPropertyType.Integer;
                case ScalarType.Date:
                    return ElasticMappingPropertyType.Date;
                default:
                    return ElasticMappingPropertyType.Text;
            }
        }

        private void AddProperty(List<ElasticMappingProperty> properties, string[] nameParts, ElasticMappingPropertyType propertyType)
        {
            if (nameParts.Length == 0) throw new ArgumentException("nameParts should not be empty");
            var name = nameParts[0];
            var existingProperty = properties.SingleOrDefault(p => p.Name == name);
            if (existingProperty == null)
            {
                var mappingProperty = new ElasticMappingProperty { Name = name };
                if (nameParts.Length == 1)
                {
                    mappingProperty.Type = propertyType;
                }
                else
                {
                    mappingProperty.Type = ElasticMappingPropertyType.Nested;
                    AddProperty(mappingProperty.Properties, nameParts.Skip(1).ToArray(), propertyType);
                }
                properties.Add(mappingProperty);
            }
            else
            {
                AddProperty(existingProperty.Properties, nameParts.Skip(1).ToArray(), propertyType);
            }
        }
    }
}
