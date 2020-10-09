using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class KeywordProperty : ElasticMappingProperty
    {
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Keyword;

        public bool SupportsNullValue { get; private set; }

        private KeywordProperty() { }

        public static ElasticMappingProperty Create(string dotName, bool supportsNullValue)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return NestedProperty.Create(splitted[0], new List<ElasticMappingProperty>
                    {
                        Create(string.Join('.', splitted.Skip(1)), supportsNullValue)
                    });
            }
            var res = new KeywordProperty
            {
                Name = splitted[0],
                SupportsNullValue = supportsNullValue
            };
            return res;
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (SupportsNullValue)
            {
                result["null_value"] = ElasticManager.NullValue;
            }
        }
    }
}
