namespace Iis.Elastic.ElasticMappingProperties
{
    public class GeoPointProperty : ElasticMappingProperty
    {
        private GeoPointProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.GeoPoint;

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(propertyName, (propName) => new GeoPointProperty { Name = propName }, (propName) => Create(propName));
        }
    }
}