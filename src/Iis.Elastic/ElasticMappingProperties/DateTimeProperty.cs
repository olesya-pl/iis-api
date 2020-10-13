using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public abstract class DateTimeProperty : ElasticMappingProperty
    {
        protected List<string> Formats = new List<string>();

        protected DateTimeProperty() { }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (Formats.Any())
            {
                result["format"] = string.Join("||", Formats);
            }
        }
    }

    public class DateProperty : DateTimeProperty
    {
        private DateProperty() { }
        
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Date;

        public static ElasticMappingProperty Create(string dotName, IEnumerable<string> formats)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return NestedProperty.Create(splitted[0], new List<ElasticMappingProperty>
                    {
                        Create(string.Join('.', splitted.Skip(1)), formats)
                    });
            }
            var res = new DateProperty()
            {
                Name = splitted[0]
            };
            if (formats != null && formats.Any())
            {
                res.Formats.AddRange(formats);
            }
            return res;
        }
    }

    public class DateRangeProperty : DateTimeProperty
    {
        private DateRangeProperty() { }
        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.DateRange;

        public static ElasticMappingProperty Create(string dotName, IEnumerable<string> formats)
        {
            var splitted = dotName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                return NestedProperty.Create(splitted[0], new List<ElasticMappingProperty>
                    {
                        Create(string.Join('.', splitted.Skip(1)), formats)
                    });
            }
            var res = new DateRangeProperty()
            {
                Name = splitted[0]
            };
            if (formats != null && formats.Any())
            {
                res.Formats.AddRange(formats);
            }
            return res;
        }
    }
}
