using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class GeoPointProperty : ElasticMappingProperty
    {
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.GeoPoint;

        private GeoPointProperty() { }

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(propertyName, (propName) => new GeoPointProperty { Name = propName }, (propName) => Create(propName));
        }

        protected override void PopulatePropertyIntoJObject(JObject result) { }
    }
}