using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology
{
    public interface IGeoCoordinates
    {
        decimal Latitude { get; }
        decimal Longitude { get; }
    }
}
