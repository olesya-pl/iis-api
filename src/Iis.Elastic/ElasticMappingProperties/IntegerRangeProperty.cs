using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class IntegerRangeProperty : ElasticMappingProperty
    {
        private IntegerRangeProperty() { }

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(
                propertyName,
                (propName) => new IntegerRangeProperty { Name = propName },
                (propName) => Create(propName)
            );
        }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.IntegerRange;

        protected override void PopulatePropertyIntoJObject(JObject result) { }
    }
}
