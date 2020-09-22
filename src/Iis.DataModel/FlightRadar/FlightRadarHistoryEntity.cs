using System;

namespace Iis.DataModel.FlightRadar
{
    public class FlightRadarHistoryEntity : BaseEntity
    {
        public decimal Lat { get; set; }
        public decimal Long { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string ICAO { get; set; }
        public Guid NodeId { get; set; }
        public NodeEntity Node { get; set; }
        public string ExternalId { get; set; }
    }
}
