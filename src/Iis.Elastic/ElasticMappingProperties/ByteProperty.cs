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
            return CreateWithNestedProperty(propertyName, (propName) => new ByteProperty { Name = propName }, (propName) => Create(propName));
        }
    }
}
