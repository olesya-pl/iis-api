using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IAttributeInfoItem
    {
        string DotName { get; }
        ScalarType ScalarType { get; }
        IEnumerable<string> AliasesList { get; }
        bool IsAggregated { get; }
    }
}
