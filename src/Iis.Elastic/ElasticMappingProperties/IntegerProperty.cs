using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class IntegerProperty : ElasticMappingProperty
    {
        private IntegerProperty() { }

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(propertyName, (propName) => new IntegerProperty { Name = propName }, (propName) => Create(propName));
        }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Integer;
        protected override void PopulatePropertyIntoJObject(JObject result) { }
    }
}
