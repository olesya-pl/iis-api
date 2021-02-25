using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class ByteProperty : ElasticMappingProperty
    {
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Byte;
        private ByteProperty() { }
        protected override void PopulatePropertyIntoJObject(JObject result) { }
        public static ElasticMappingProperty Create(string propertyName)
        {
            var propertyNameList = propertyName.Split(PropertyNameSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (!propertyNameList.Any()) throw new ArgumentException($"Wrong property name {propertyName}");

            if (propertyNameList.Length == 1) return new ByteProperty { Name = propertyNameList.First() };

            var newPropertyName = string.Join(PropertyNameSeparator, propertyNameList.Skip(1));

            return NestedProperty.Create(propertyNameList.First(), Create(newPropertyName));
        }
    }
}
