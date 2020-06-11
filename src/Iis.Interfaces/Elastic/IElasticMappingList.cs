using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticMappingList
    {
        IReadOnlyList<IElasticMappingItem> Items { get; set; }
    }
}
