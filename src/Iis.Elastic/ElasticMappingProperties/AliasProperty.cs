using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class AliasProperty : ElasticMappingProperty
    {
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Alias;

        public string Path { get; private set; }

        private AliasProperty() { }
        
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
            result["path"] = Path;
        }
    }
}
