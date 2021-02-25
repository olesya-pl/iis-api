using Iis.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Iis.Elastic
{
    public abstract class ElasticMappingProperty
    {
        protected const char PropertyNameSeparator = '.';
        public string Name { get; set; }
        public abstract ElasticMappingPropertyType Type { get; }
        public List<ElasticMappingProperty> Properties { get; set; } = new List<ElasticMappingProperty>();
        protected abstract void PopulatePropertyIntoJObject(JObject result);

        public JObject ToJObject()
        {
            var result = new JObject();
            PopulatePropertyIntoJObject(result);
            if (Type != ElasticMappingPropertyType.Nested || Properties.Count == 0)
            {
                result["type"] = Type.ToString().ToUnderscore();
            }
            return result;
        }

        public override string ToString() => Name;
    }


}
