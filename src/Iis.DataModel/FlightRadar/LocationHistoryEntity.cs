using System;

namespace Iis.DataModel.FlightRadar
{
    public class LocationHistoryEntity : BaseEntity
    {
        public decimal Lat { get; set; }
        public decimal Long { get; set; }
        public DateTime RegisteredAt { get; set; }
        public Guid NodeId { get; set; }
        public Guid EntityId { get; set; }
        public NodeEntity Node { get; set; }
        public NodeEntity Entity { get; set; }
        public string ExternalId { get; set; }
    }
}
