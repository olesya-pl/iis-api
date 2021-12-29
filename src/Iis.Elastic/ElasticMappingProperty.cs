using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Elastic.ElasticMappingProperties;
using Iis.Utility;
using Newtonsoft.Json.Linq;
namespace Iis.Elastic
{
    public abstract class ElasticMappingProperty
    {
        protected const char PropertyNameSeparator = '.';
        private const string TypePropertyName = "type";
        public string Name { get; set; }
        public abstract ElasticMappingPropertyType Type { get; }
        public List<ElasticMappingProperty> Properties { get; set; } = new List<ElasticMappingProperty>();
        public override string ToString() => Name;

        public JObject ToJObject()
        {
            var result = new JObject();
            PopulatePropertyIntoJObject(result);
            if (!IsNonEmptyNestedProperty() || IsDenseVectorContainer())
            {
                result[TypePropertyName] = Type.ToString().ToUnderscore();
            }

            return result;
        }

        protected static ElasticMappingProperty CreateWithNestedProperty(string propertyName, Func<string, ElasticMappingProperty> newPropertyFunc, Func<string, ElasticMappingProperty> nestedPropertyFunc)
        {
            var propertyNameList = propertyName.Split(PropertyNameSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (propertyNameList.Length == 1) return newPropertyFunc(propertyNameList.First());

            var newPropertyName = string.Join(PropertyNameSeparator, propertyNameList.Skip(1));

            return NestedProperty.Create(propertyNameList.First(), nestedPropertyFunc(newPropertyName));
        }

        protected abstract void PopulatePropertyIntoJObject(JObject result);

        private bool IsDenseVectorContainer()
        {
            return Properties.Any(p => p.Type == ElasticMappingPropertyType.DenseVector);
        }

        private bool IsNonEmptyNestedProperty()
        {
            return Type == ElasticMappingPropertyType.Nested && Properties.Count != 0;
        }
    }
}
