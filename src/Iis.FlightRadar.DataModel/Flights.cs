using System;
using System.Collections.Generic;

namespace Iis.FlightRadar.DataModel
{
    public partial class Flights
    {
        public Flights()
        {
            Routes = new HashSet<Routes>();
        }

        public int Id { get; set; }
        public string FlightNo { get; set; }
        public DateTime? ScheduledDepartureAt { get; set; }
        public DateTime? ScheduledArrivalAt { get; set; }
        public DateTime? RealDepartureAt { get; set; }
        public DateTime? RealArrivalAt { get; set; }
        public string ExternalId { get; set; }
        public string Meta { get; set; }
        public int? ArrivalAirportId { get; set; }
        public int? DepartureAirportId { get; set; }
        public int? PlaneId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Airports ArrivalAirport { get; set; }
        public virtual Airports DepartureAirport { get; set; }
        public virtual Aircraft Plane { get; set; }
        public virtual ICollection<Routes> Routes { get; set; }
    }
}
