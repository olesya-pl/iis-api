using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology
{
    public class GeoCoordinates : IGeoCoordinates
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public GeoCoordinates() { }

        public GeoCoordinates(decimal latitude, decimal longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
