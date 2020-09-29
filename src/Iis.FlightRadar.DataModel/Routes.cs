using System;
using System.Collections.Generic;

namespace Iis.FlightRadar.DataModel
{
    public partial class Routes
    {
        public int Id { get; set; }
        public string Callsign { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Altitude { get; set; }
        public int? Track { get; set; }
        public double? Speed { get; set; }
        public DateTime? TimeNow { get; set; }
        public string SquawkCode { get; set; }
        public int? FlightId { get; set; }

        public virtual Flights Flight { get; set; }
    }
}
