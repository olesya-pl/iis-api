using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.ElasticMappingProperties
{
    public abstract class DateTimeProperty : ElasticMappingProperty
    {
        protected const string FormatPropertyName = "format";
        protected List<string> Formats = new List<string>();

        protected DateTimeProperty() { }

        protected override void PopulatePropertyIntoJObject(JObject result)
        {
            if (Formats.Any())
            {
                result[FormatPropertyName] = string.Join("||", Formats);
            }
        }
    }

    public class DateProperty : DateTimeProperty
    {
        private DateProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.Date;

        public static ElasticMappingProperty Create(string propertyName, IReadOnlyCollection<string> formats = null)
        {
            Func<string, ElasticMappingProperty> newPropFunc = (propName) => {
                var property = new DateProperty{ Name = propName};
                if(formats != null && formats.Any()) property.Formats.AddRange(formats);
                return property;
            };

            return CreateWithNestedProperty(
                propertyName,
                newPropFunc,
                (propName) => Create(propName, formats)
            );
        }
    }

    public class DateRangeProperty : DateTimeProperty
    {
        private DateRangeProperty() { }

        public override ElasticMappingPropertyType Type => ElasticMappingPropertyType.DateRange;

        public static ElasticMappingProperty Create(string propertyName, IReadOnlyCollection<string> formats = null)
        {
            Func<string, ElasticMappingProperty> newPropFunc = (propName) => {
                var property = new DateRangeProperty { Name = propName };
                if (formats != null && formats.Any()) property.Formats.AddRange(formats);
                return property;
            };

            return CreateWithNestedProperty(
                propertyName,
                newPropFunc,
                (propName) => Create(propName, formats)
            );
        }
    }
}
