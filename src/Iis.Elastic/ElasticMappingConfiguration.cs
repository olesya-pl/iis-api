using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Elastic
{
    public class ElasticMappingConfiguration
    {
        public List<ElasticMappingProperty> Properties { get; } = new List<ElasticMappingProperty>();

        public ElasticMappingConfiguration(List<ElasticMappingProperty> properties)
        {
            Properties = properties;
        }
        public ElasticMappingConfiguration(IAttributeInfoList attributeInfo)
        {
            foreach (var item in attributeInfo.Items)
            {
                var nameParts = item.DotName.Split('.');
                var mappingType = ToMappingType(item.ScalarType);
                AddProperty(Properties, nameParts, mappingType);
            }
            foreach (var item in attributeInfo.Items)
            {
                AddAlias(Properties, item);
            }
        }

        public JObject ToJObject()
        {
            var jProperties = new JObject();
            foreach (var property in Properties)
            {
                jProperties[property.Name] = property.ToJObject();
            }
            var inner = new JObject();
            inner["properties"] = jProperties;
            var result = new JObject();
            result["mappings"] = inner;
            return result;
        }

        public static ElasticMappingPropertyType ToMappingType(ScalarType scalarType)
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
                var isNestedProperty = nameParts.Length > 1;
                if (isNestedProperty)
                {
                    mappingProperty.Type = ElasticMappingPropertyType.Nested;
                    AddProperty(mappingProperty.Properties, nameParts.Skip(1).ToArray(), propertyType);
                }
                else
                {
                    mappingProperty.Type = propertyType;

                    if(propertyType == ElasticMappingPropertyType.Date)
                    {
                        mappingProperty.AddFormats(ElasticConfiguration.DefaultDateFormats);
                    }
                }
                properties.Add(mappingProperty);
            }
            else
            {
                if (nameParts.Length > 1)
                {
                    AddProperty(existingProperty.Properties, nameParts.Skip(1).ToArray(), propertyType);
                }
            }
        }

        private void AddAlias(List<ElasticMappingProperty> properties, IAttributeInfoItem attributeInfoItem)
        {
            if (attributeInfoItem.AliasesList == null) return;

            foreach (var aliasName in attributeInfoItem.AliasesList)
            {
                var mappingProperty = new ElasticMappingProperty
                {
                    Name = aliasName,
                    Type = ElasticMappingPropertyType.Alias,
                    Path = attributeInfoItem.DotName
                };
                properties.Add(mappingProperty);
            }
        }
    }
}
