using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticMappingItem
    {
        string PropertyDotName { get; set; }
        ElasticMappingItemType DataType { get; set; }
    }
}
