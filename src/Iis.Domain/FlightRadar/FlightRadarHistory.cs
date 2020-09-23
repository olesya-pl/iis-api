using System;

namespace Iis.Domain.FlightRadar
{
    public class FlightRadarHistory
    {
        public decimal Lat { get; set; }
        public decimal Long { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string ExternalId { get; set; }
    }
}
