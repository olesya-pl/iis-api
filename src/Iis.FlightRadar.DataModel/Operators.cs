using System;
using System.Collections.Generic;

namespace Iis.FlightRadar.DataModel
{
    public partial class Operators
    {
        public Operators()
        {
            Aircraft = new HashSet<Aircraft>();
        }

        public int Id { get; set; }
        public string Icao { get; set; }
        public string Iata { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Country { get; set; }
        public string About { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Aircraft> Aircraft { get; set; }
    }
}
