using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class DenseVectorProperty : ElasticMappingProperty
    {
        private const string DimensionsPropName = "dims";

        private DenseVectorProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.DenseVector;

        public int Dimensions { get; private set; }

        public static ElasticMappingProperty Create(string propertyName, int dimensions)
        {
            return CreateWithNestedProperty(
                propertyName,
                (propName) => new DenseVectorProperty{ Name = propertyName, Dimensions = dimensions},
                (propName) => Create(propName, dimensions)
            );
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            result[DimensionsPropName] = Dimensions;
        }
    }
}
