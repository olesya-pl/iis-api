using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class DenseVectorProperty : ElasticMappingProperty
    {
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.DenseVector;

        public int Dimensions { get; private set; }

        private DenseVectorProperty() { }

        public static ElasticMappingProperty Create(string dotName, int dimensions)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return NestedProperty.Create(splitted[0], new List<ElasticMappingProperty>
                    {
                        Create(string.Join('.', splitted.Skip(1)), dimensions)
                    });
            }
            return new DenseVectorProperty
            {
                Name = splitted[0],
                Dimensions = dimensions
            };
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            result["dims"] = Dimensions;
        }
    }
}
