using System;
using System.Linq;
using Iis.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Iis.Elastic.ElasticMappingProperties;
namespace Iis.Elastic
{
    public abstract class ElasticMappingProperty
    {
        protected const char PropertyNameSeparator = '.';
        public string Name { get; set; }
        public abstract ElasticMappingPropertyType Type { get; }
        public List<ElasticMappingProperty> Properties { get; set; } = new List<ElasticMappingProperty>();
        protected abstract void PopulatePropertyIntoJObject(JObject result);

        public JObject ToJObject()
        {
            var result = new JObject();
            PopulatePropertyIntoJObject(result);
            if (Type != ElasticMappingPropertyType.Nested || Properties.Count == 0)
            {
                result["type"] = Type.ToString().ToUnderscore();
            }
            return result;
        }

        public override string ToString() => Name;

        protected static ElasticMappingProperty CreateWithNestedProperty(string propertyName, Func<string, ElasticMappingProperty> newPropertyFunc, Func<string, ElasticMappingProperty> nestedPropertyFunc)
        {
            var propertyNameList = propertyName.Split(PropertyNameSeparator, StringSplitOptions.RemoveEmptyEntries);

            if(propertyNameList.Length == 1) return newPropertyFunc(propertyNameList.First());

            var newPropertyName = string.Join(PropertyNameSeparator, propertyNameList.Skip(1));

            return NestedProperty.Create(propertyNameList.First(), nestedPropertyFunc(newPropertyName));
        }
    }
}
