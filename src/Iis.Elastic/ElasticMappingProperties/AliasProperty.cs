using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class AliasProperty : ElasticMappingProperty
    {
        private const string PathPropertyName = "path";

        private AliasProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Alias;

        public string Path { get; private set; }

        public static ElasticMappingProperty Create(string name, string path)
        {
            return new AliasProperty
            {
                Name = name,
                Path = path
            };
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            result[PathPropertyName] = Path;
        }
    }
}
