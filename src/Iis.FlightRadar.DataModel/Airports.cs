using System;
using System.Collections.Generic;

namespace Iis.FlightRadar.DataModel
{
    public partial class Airports
    {
        public Airports()
        {
            FlightsArrivalAirport = new HashSet<Flights>();
            FlightsDepartureAirport = new HashSet<Flights>();
        }

        public int Id { get; set; }
        public string Icao { get; set; }
        public string Iata { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string CountryCodeLong { get; set; }
        public string City { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Altitude { get; set; }
        public string Website { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Flights> FlightsArrivalAirport { get; set; }
        public virtual ICollection<Flights> FlightsDepartureAirport { get; set; }
    }
}
