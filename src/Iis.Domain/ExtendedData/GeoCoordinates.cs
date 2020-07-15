using Iis.Interfaces.Ontology;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.ExtendedData
{
    public class GeoCoordinates: IGeoCoordinates
    {
        public decimal Latitude { get; set; }
        public decimal Langitude { get; set; }
        public GeoCoordinates(decimal latitude, decimal langitude)
        {
            Latitude = latitude;
            Langitude = langitude;
        }
    }
}
