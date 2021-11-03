using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class IntegerRangeProperty : ElasticMappingProperty
    {
        private IntegerRangeProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.IntegerRange;

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(
                propertyName,
                (propName) => new IntegerRangeProperty { Name = propName },
                (propName) => Create(propName));
        }

        protected override void PopulatePropertyIntoJObject(JObject result) { }
    }
}
