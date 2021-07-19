using System;
namespace Iis.Services.Contracts.Dtos
{
    public class SituationNodeDto
    {
        public GeometryDto Geometry { get; }
        public AttributesDto Attributes { get; }
        public SituationNodeDto(AttributesDto attributes, GeometryDto geometry)
        {
            Geometry = geometry;
            Attributes = attributes;
        }
    }

    public class GeometryDto
    {
        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public GeometryDto(decimal latitude, decimal longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    public class AttributesDto
    {
        public Guid Id { get; }
        public string Title { get; }
        public string TypeTitle { get; }
        public string TypeName { get; }
        public string Sidc { get; }
        public DateTime LastConfirmedAt { get; }
        public AttributesDto(Guid id, string typeTitle, string typeName, string title, string sidc, DateTime lastConfirmedAt)
        {
            Id = id;
            TypeTitle = typeTitle;
            TypeName = typeName;
            Title = title;
            Sidc = sidc;
            LastConfirmedAt = lastConfirmedAt;
        }
    }
}