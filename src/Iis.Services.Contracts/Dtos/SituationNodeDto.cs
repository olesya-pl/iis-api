using System;
using System.Collections.Generic;
namespace Iis.Services.Contracts.Dtos
{
    public class SituationNodeDto
    {
        public GeometryDto Geometry { get; }
        public AttributesDto Attributes { get; }
        public SituationNodeDto(GeometryDto geometry, AttributesDto attributes)
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
        public ObjectDto ObjectOfStudy { get; }
        public SignDto Sign { get; }
        public MaterialDto Material { get; }
        public DateTime RegisteredAt { get; }
        public AttributesDto(ObjectDto objectOfStudy, SignDto sign, MaterialDto material, DateTime registeredAt)
        {
            ObjectOfStudy = objectOfStudy;
            Sign = sign;
            Material = material;
            RegisteredAt = registeredAt;
        }
    }

    public class ObjectDto : BaseObjectDto
    {
        public string Title { get; }
        public string Sidc { get; set; }
        public ObjectDto(Guid id, string typeName, string typeTitle, string title, string sidc)
        : base(id, typeName, typeTitle)
        {
            Title = title;
            Sidc = sidc;
        }

    }

    public class SignDto : BaseObjectDto
    {
        public string Value { get; }

        public SignDto(Guid id, string typeName, string typeTitle, string value)
        : base(id, typeName, typeTitle)
        {
            Value = value;
        }
    }

    public class MaterialDto
    {
        public Guid Id { get; }
        public string Type { get; }
        public string Source { get; }
        public string Title { get; }
        public DateTime CreatedDate { get; }
        public DateTime? RegistrationDate { get; }
        public MaterialDto(Guid id, string type, string source, string title, DateTime createdDate, DateTime? registrationDate)
        {
            Id = id;
            Type = type;
            Source = source;
            Title = title;
            CreatedDate = createdDate;
            RegistrationDate = registrationDate;
        }
    }

    public abstract class BaseObjectDto
    {
        public Guid Id { get; }
        public string TypeTitle { get; }
        public string TypeName { get; }
        protected BaseObjectDto(Guid id, string typeName, string typeTitle)
        {
            Id = id;
            TypeName = typeName;
            TypeTitle = typeTitle;
        }
    }
}