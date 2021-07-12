using System;
using System.Linq;

namespace Iis.CoordinatesEventHandler.Types
{
    public class Coordinate
    {
        public DateTime RegisteredAt { get; } = DateTime.UtcNow;
        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public bool IsValid { get; }

        public Coordinate(string coordinates)
        {
            if (string.IsNullOrWhiteSpace(coordinates)) return;

            var valueList = coordinates
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .ToArray();

            if (valueList.Length != 2) return;

            var parseResult = Decimal.TryParse(valueList[0], out decimal latitude)
                            & Decimal.TryParse(valueList[1], out decimal longitude);

            if (!parseResult) return;

            IsValid = true;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}