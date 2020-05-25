using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Elastic
{
    public enum ElasticMappingPropertyType
    {
        Text,
        Integer,
        Date,
        Nested
    }
    public class ElasticMappingProperty
    {
        public string Name { get; set; }
        public ElasticMappingPropertyType Type { get; set; }
        public List<ElasticMappingProperty> Properties { get; set; } = new List<ElasticMappingProperty>();
    }
}
