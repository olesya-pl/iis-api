using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class ByteProperty : ElasticMappingProperty
    {
        private ByteProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Byte;

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(propertyName, (propName) => new ByteProperty { Name = propName }, (propName) => Create(propName));
        }

        protected override void PopulatePropertyIntoJObject(JObject result) { }
    }
}
