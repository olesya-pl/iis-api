using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public class TextProperty : ElasticMappingProperty
    {
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Text;

        public string TermVector { get; private set; }

        private TextProperty() { }

        public static ElasticMappingProperty Create(string dotName, string termVector)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return NestedProperty.Create(splitted[0], new List<ElasticMappingProperty>
                    {
                        Create(string.Join('.', splitted.Skip(1)), termVector)
                    });
            }
            return new TextProperty
            {
                Name = splitted[0],
                TermVector = termVector
            };
        }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (!string.IsNullOrWhiteSpace(TermVector))
            {
                result["term_vector"] = TermVector;
            }
        }
    }
}
