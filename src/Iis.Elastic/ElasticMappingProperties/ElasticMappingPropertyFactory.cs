using System;
using System.Collections.Generic;
using Iis.DbLayer.Repositories;

namespace Iis.Elastic.ElasticMappingProperties
{
    public static class ElasticMappingPropertyFactory
    {
        public static ElasticMappingProperty Create(string[] nameParts, ElasticMappingPropertyType propertyType)
        {
            var name = nameParts[0];
            var isNestedProperty = nameParts.Length > 1;
            if (isNestedProperty)
            {
                return NestedProperty.Create(name, new List<ElasticMappingProperty>());
            }
            else
            {
                switch (propertyType)
                {
                    case ElasticMappingPropertyType.Text:
                        return TextProperty.Create(name, null);
                    case ElasticMappingPropertyType.Integer:
                        return IntegerRangeProperty.Create(name);
                    case ElasticMappingPropertyType.Date:
                        return DateProperty.Create(name, ElasticConfiguration.DefaultDateFormats);
                    case ElasticMappingPropertyType.Keyword:
                        return KeywordProperty.Create(name, false);
                    case ElasticMappingPropertyType.DenseVector:
                        return DenseVectorProperty.Create(name, MaterialDocument.ImageVectorDimensionsCount);
                    case ElasticMappingPropertyType.DateRange:
                        return DateRangeProperty.Create(name, ElasticConfiguration.DefaultDateFormats);
                    case ElasticMappingPropertyType.Nested:
                        return NestedProperty.Create(name, new List<ElasticMappingProperty>());
                    case ElasticMappingPropertyType.IntegerRange:
                        return IntegerRangeProperty.Create(name);
                    case ElasticMappingPropertyType.FloatRange:
                        return FloatRangeProperty.Create(name);
                    default:
                        throw new NotSupportedException($"Creating instance of type {propertyType} is not supported");
                }
            }
        }
    }
}
