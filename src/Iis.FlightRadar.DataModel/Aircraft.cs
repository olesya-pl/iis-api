using System;
using System.Collections.Generic;

namespace Iis.FlightRadar.DataModel
{
    public partial class Aircraft
    {
        public Aircraft()
        {
            Flights = new HashSet<Flights>();
        }

        public int Id { get; set; }
        public string RegistrationNumber { get; set; }
        public string Icao { get; set; }
        public string Model { get; set; }
        public string DetailedModel { get; set; }
        public string Photo { get; set; }
        public string Type { get; set; }
        public int? OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Operators Owner { get; set; }
        public virtual ICollection<Flights> Flights { get; set; }
    }
}
