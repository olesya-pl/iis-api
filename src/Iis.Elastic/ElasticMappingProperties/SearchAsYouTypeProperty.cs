using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class SearchAsYouTypeProperty : ElasticMappingProperty
    {
        private SearchAsYouTypeProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.SearchAsYouType;

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(propertyName, (propName) => new SearchAsYouTypeProperty { Name = propName }, (propName) => Create(propName));
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
        }
    }
}
