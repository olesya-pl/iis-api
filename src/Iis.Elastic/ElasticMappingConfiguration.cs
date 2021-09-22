using Iis.Elastic.ElasticMappingProperties;
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
                AddProperty(Properties, nameParts, mappingType, item.IsAggregated);
            }
            foreach (var item in attributeInfo.Items)
            {
                AddAlias(Properties, item);
            }
        }

        public JObject ToJObject()
        {
            var result = new JObject();
            result["mappings"] = GetPropertiesJObject();
            return result;
        }

        public JObject GetPropertiesJObject()
        {
            var jProperties = new JObject();
            foreach (var property in Properties)
            {
                if (jProperties.ContainsKey(property.Name))
                {
                    (jProperties[property.Name] as JObject).Merge(property.ToJObject());
                }
                else
                {
                    jProperties[property.Name] = property.ToJObject();
                }
            }
            var inner = new JObject();
            inner["properties"] = jProperties;
            return inner;
        }


        public static ElasticMappingPropertyType ToMappingType(ScalarType scalarType)
        {
            return scalarType switch
            {
                ScalarType.Int => ElasticMappingPropertyType.Integer,
                ScalarType.IntegerRange => ElasticMappingPropertyType.IntegerRange,
                ScalarType.FloatRange => ElasticMappingPropertyType.FloatRange,
                ScalarType.Date => ElasticMappingPropertyType.Date,
                ScalarType.DateRange => ElasticMappingPropertyType.DateRange,
                ScalarType.GeoPoint => ElasticMappingPropertyType.GeoPoint,
                ScalarType.File => ElasticMappingPropertyType.Nested,
                _ => ElasticMappingPropertyType.Text,
            };
        }

        private void AddProperty(List<ElasticMappingProperty> properties, string[] nameParts, ElasticMappingPropertyType propertyType, bool isAggregated)
        {
            if (nameParts.Length == 0) throw new ArgumentException("nameParts should not be empty");
            var name = nameParts[0];
            var existingProperty = properties.SingleOrDefault(p => p.Name == name);
            if (existingProperty == null)
            {
                var mappingProperties = ElasticMappingPropertyFactory.Create(nameParts, propertyType, isAggregated);
                if (nameParts.Count() > 1)
                {
                    foreach (var mappingProperty in mappingProperties) 
                    {
                        AddProperty(mappingProperty.Properties, nameParts.Skip(1).ToArray(), propertyType, isAggregated);
                    }
                }
                properties.AddRange(mappingProperties);
            }
            else
            {
                if (nameParts.Length > 1)
                {
                    AddProperty(existingProperty.Properties, nameParts.Skip(1).ToArray(), propertyType, isAggregated);
                }
            }
        }

        private void AddAlias(List<ElasticMappingProperty> properties, IAttributeInfoItem attributeInfoItem)
        {
            if (attributeInfoItem.AliasesList == null) return;

            foreach (var aliasName in attributeInfoItem.AliasesList)
            {
                var mappingProperty = AliasProperty.Create(aliasName, attributeInfoItem.DotName);
                properties.Add(mappingProperty);
            }
        }
    }
}
