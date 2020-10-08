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
        public bool SupportsNullValue { get; private set; }
        public int? Dimensions { get; private set; }
        public string TermVector { get; private set; }

        public ElasticMappingProperty() { }

        public static ElasticMappingProperty CreateDenseVectorProperty(string dotName, int dimensions)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return new ElasticMappingProperty
                {
                    Name = splitted[0],
                    Type = ElasticMappingPropertyType.Nested,
                    Properties = new List<ElasticMappingProperty>
                    {
                        CreateDenseVectorProperty(string.Join('.', splitted.Skip(1)), dimensions)
                    }
                };
            }
            return new ElasticMappingProperty
            {
                Name = splitted[0],
                Type = ElasticMappingPropertyType.DenseVector,
                Dimensions = dimensions
            };
        }

        public static ElasticMappingProperty CreateTextProperty(string dotName, string termVector)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return new ElasticMappingProperty
                {
                    Name = splitted[0],
                    Type = ElasticMappingPropertyType.Nested,
                    Properties = new List<ElasticMappingProperty>
                    {
                        CreateTextProperty(string.Join('.', splitted.Skip(1)), termVector)
                    }
                };
            }
            return new ElasticMappingProperty
            {
                Name = splitted[0],
                Type = ElasticMappingPropertyType.Text,
                TermVector = termVector
            };
        }

        public static ElasticMappingProperty CreateDateTimeProperty(string dotName, IEnumerable<string> formats)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return new ElasticMappingProperty
                {
                    Name = splitted[0],
                    Type = ElasticMappingPropertyType.Nested,
                    Properties = new List<ElasticMappingProperty>
                    {
                        CreateDateTimeProperty(string.Join('.', splitted.Skip(1)), formats)
                    }
                };
            }
            var res = new ElasticMappingProperty
            {
                Name = splitted[0],
                Type = ElasticMappingPropertyType.Date
            };
            if (formats != null && formats.Any())
            {
               res.Formats.AddRange(formats);
            }
            return res;
        }

        public static ElasticMappingProperty CreateKeywordProperty(string dotName, bool supportsNullValue)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return new ElasticMappingProperty
                {
                    Name = splitted[0],
                    Type = ElasticMappingPropertyType.Nested,
                    Properties = new List<ElasticMappingProperty>
                    {
                        CreateKeywordProperty(string.Join('.', splitted.Skip(1)), supportsNullValue)
                    }
                };
            }
            var res = new ElasticMappingProperty
            {
                Name = splitted[0],
                Type = ElasticMappingPropertyType.Keyword,                
            };
            res.SupportsNullValue = supportsNullValue;
            return res;
        }

        public JObject ToJObject()
        {
            var result = new JObject();

            if (Type != ElasticMappingPropertyType.Nested || Properties.Count == 0)
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

            if ((Type == ElasticMappingPropertyType.Date || Type == ElasticMappingPropertyType.DateRange) 
                && Formats.Any())
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
