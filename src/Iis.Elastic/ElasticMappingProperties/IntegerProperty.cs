using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class IntegerProperty : ElasticMappingProperty
    {
        private IntegerProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Integer;

        public static ElasticMappingProperty Create(string propertyName)
        {
            return CreateWithNestedProperty(propertyName, (propName) => new IntegerProperty { Name = propName }, (propName) => Create(propName));
        }
    }
}
