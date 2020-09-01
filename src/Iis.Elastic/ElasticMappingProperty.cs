using Iis.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Elastic
{
    public class ElasticMappingProperty
    {
        public string Name { get; set; }
        public ElasticMappingPropertyType Type { get; set; }
        public List<string> Formats { get; } = new List<string>();
        public string Path { get; set; }
        public List<ElasticMappingProperty> Properties { get; set; } = new List<ElasticMappingProperty>();
        public bool SupportsNullValue { get; }
        public int? Dimensions { get; }
        public string TermVector { get; }

        public ElasticMappingProperty() { }

        public ElasticMappingProperty(string dotName,
            ElasticMappingPropertyType type,
            bool supportsNullValue = false,
            IEnumerable<string> formats = null,
            int? dimensions = null,
            string termVector = null)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            Name = splitted[0];
            if (splitted.Length == 1)
            {
                Type = type;
            }
            else
            {
                Type = ElasticMappingPropertyType.Nested;
                Properties = new List<ElasticMappingProperty>
                {
                    new ElasticMappingProperty(string.Join('.', splitted.Skip(1)), type, formats:formats)
                };
            }
            SupportsNullValue = supportsNullValue;

            if (formats != null && formats.Any())
            {
                Formats.AddRange(formats);
            }
            Dimensions = dimensions;

            TermVector = termVector;
        }

        public JObject ToJObject()
        {
            var result = new JObject();

            if (Type != ElasticMappingPropertyType.Nested)
            {
                result["type"] = Type.ToString().ToUnderscore();

                if (Dimensions.HasValue)
                {
                    result["dims"] = Dimensions;
                }
            }

            if (SupportsNullValue)
            {
                result["null_value"] = ElasticManager.NullValue;
            }

            if (Type == ElasticMappingPropertyType.Alias)
            {
                result["path"] = Path;
            }

            if (Type is ElasticMappingPropertyType.Text && !string.IsNullOrWhiteSpace(TermVector))
            {
                result["term_vector"] = TermVector;
            }

            if (Type == ElasticMappingPropertyType.Date && Formats.Any())
            {
                result["format"] = string.Join("||", Formats);
            }

            if (Properties.Count > 0)
            {
                var jProperties = new JObject();
                foreach (var property in Properties)
                {
                    jProperties[property.Name] = property.ToJObject();
                }
                result["properties"] = jProperties;
            }
            return result;
        }

        public void AddFormats(string format)
        {
            if(string.IsNullOrWhiteSpace(format))
            {
                Formats.Add(format);
            }
        }
        public void AddFormats(IEnumerable<string> format)
        {
            if(format != null && format.Any())
            {
                Formats.AddRange(format);
            }
        }

        public override string ToString() => Name;
    }


}
