using System;
using System.Collections.Generic;
using Iis.Elastic.Entities;
using Iis.Elastic.SearchQueryExtensions;

namespace Iis.Elastic.ElasticMappingProperties
{
    public static class ElasticMappingPropertyFactory
    {
        public static List<ElasticMappingProperty> Create(string[] nameParts, ElasticMappingPropertyType propertyType, bool isAggregated)
        {
            var name = nameParts[0];
            var isNestedProperty = nameParts.Length > 1;
            if (isNestedProperty)
            {
                return new List<ElasticMappingProperty> { NestedProperty.Create(name, new List<ElasticMappingProperty>()) };
            }
            else
            {
                switch (propertyType)
                {
                    case ElasticMappingPropertyType.Text:
                        var res = new List<ElasticMappingProperty>() { TextProperty.Create(name, true, true) };
                        if (isAggregated)
                        {
                            res.Add(KeywordProperty.Create($"{name}{SearchQueryExtension.AggregateSuffix}", false));
                        }
                        return res;
                    case ElasticMappingPropertyType.Integer:
                        return new List<ElasticMappingProperty> { IntegerProperty.Create(name) };
                    case ElasticMappingPropertyType.Date:
                        return new List<ElasticMappingProperty> { DateProperty.Create(name, ElasticConfiguration.DefaultDateFormats) };
                    case ElasticMappingPropertyType.Keyword:
                        return new List<ElasticMappingProperty> { KeywordProperty.Create(name, false) };
                    case ElasticMappingPropertyType.DenseVector:
                        return new List<ElasticMappingProperty> { DenseVectorProperty.Create(name, MaterialDocument.ImageVectorDimensionsCount) };
                    case ElasticMappingPropertyType.DateRange:
                        return new List<ElasticMappingProperty> { DateRangeProperty.Create(name, ElasticConfiguration.DefaultDateFormats) };
                    case ElasticMappingPropertyType.Nested:
                        return new List<ElasticMappingProperty> { NestedProperty.Create(name, new List<ElasticMappingProperty>()) };
                    case ElasticMappingPropertyType.IntegerRange:
                        return new List<ElasticMappingProperty> { IntegerRangeProperty.Create(name) };
                    case ElasticMappingPropertyType.FloatRange:
                        return new List<ElasticMappingProperty> { FloatRangeProperty.Create(name) };
                    case ElasticMappingPropertyType.GeoPoint:
                        return new List<ElasticMappingProperty> { GeoPointProperty.Create(name) };
                    default:
                        throw new NotSupportedException($"Creating instance of type {propertyType} is not supported");
                }
            }
        }
    }
}
