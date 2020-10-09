using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class FloatRangeProperty : ElasticMappingProperty
    {
        private FloatRangeProperty() { }

        public static ElasticMappingProperty Create(string dotName)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return NestedProperty.Create(splitted[0], new List<ElasticMappingProperty>
                    {
                        Create(string.Join('.', splitted.Skip(1)))
                    });
            }
            return new FloatRangeProperty
            {
                Name = splitted[0],
            };
        }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.FloatRange;

        protected override void PopulatePropertyIntoJObject(JObject result)
        {

        }
    }
}
