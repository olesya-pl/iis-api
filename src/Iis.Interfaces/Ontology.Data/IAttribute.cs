using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IAttribute: IAttributeBase
    {
        INode Node { get; }
        ScalarType ScalarType { get; }
        GeoCoordinates ValueAsGeoCoordinates { get; }
    }
}
