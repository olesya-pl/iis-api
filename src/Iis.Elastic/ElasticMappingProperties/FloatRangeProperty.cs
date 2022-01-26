namespace Iis.Elastic.ElasticMappingProperties
{
    public class FloatRangeProperty : ElasticMappingProperty
    {
        private FloatRangeProperty() { }

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(
                propertyName,
                (propName) => new FloatRangeProperty { Name = propName },
                (propName) => Create(propName));
        }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.FloatRange;
    }
}
