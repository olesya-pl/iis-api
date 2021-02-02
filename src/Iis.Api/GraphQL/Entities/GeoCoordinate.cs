namespace Iis.Api.GraphQL.Entities
{
    public class GeoCoordinate
    {
        public decimal Lat { get; set; }

        public decimal Long { get; set; }

        public string Label { get; set; }

        public string PropertyName { get; set; }
    }
}
