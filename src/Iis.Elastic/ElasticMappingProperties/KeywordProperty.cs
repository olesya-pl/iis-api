using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class KeywordProperty : ElasticMappingProperty
    {
        private const string NullValuePropertyName = "null_value";

        private bool _supportsNullValue;

        private KeywordProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Keyword;

        public static ElasticMappingProperty Create(string propertyName, bool supportsNullValue)
        {
            return CreateWithNestedProperty(
                propertyName,
                (propName) => new KeywordProperty { Name = propName, _supportsNullValue = supportsNullValue },
                (propName) => Create(propName, supportsNullValue)
            );
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (_supportsNullValue)
            {
                result[NullValuePropertyName] = ElasticManager.NullValue;
            }
        }
    }
}
