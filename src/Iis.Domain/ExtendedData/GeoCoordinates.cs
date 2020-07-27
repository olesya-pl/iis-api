using Iis.Interfaces.Ontology;

namespace Iis.Domain.ExtendedData
{
    public class GeoCoordinates: IGeoCoordinates
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public GeoCoordinates(decimal latitude, decimal longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
