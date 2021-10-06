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

    public class GeometryDto : IEquatable<GeometryDto>
    {
        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public GeometryDto(decimal latitude, decimal longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public bool Equals(GeometryDto other)
        {
            if (other is null) return false;

            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude);
        }
    }

    public class AttributesDto
    {
        public IReadOnlyCollection<ObjectDto> ObjectOfStudyCollection { get; }
        public IReadOnlyCollection<SignDto> SignCollection { get; }
        public IReadOnlyCollection<MaterialDto> MaterialCollection { get; }
        public AttributesDto(IReadOnlyCollection<ObjectDto> objectOfStudyCollection, IReadOnlyCollection<SignDto> signCollection, IReadOnlyCollection<MaterialDto> materialCollection)
        {
            ObjectOfStudyCollection = objectOfStudyCollection;
            SignCollection = signCollection;
            MaterialCollection = materialCollection;
        }
    }

    public class ObjectDto : BaseObjectDto
    {
        public string Title { get; }
        public string Sidc { get; }
        public string NickName { get; }
        public Guid SignId { get; }
        public ObjectDto(Guid id, string typeName, string typeTitle, string title, string sidc, string nickName, Guid signId, Guid? materialId, DateTime? materialRegistrationDate)
        : base(id, typeName, typeTitle, materialId, materialRegistrationDate)
        {
            Title = title;
            Sidc = sidc;
            NickName = nickName;
            SignId = signId;
        }

    }

    public class SignDto : BaseObjectDto
    {
        public string Value { get; }

        public SignDto(Guid id, string typeName, string typeTitle, string value, Guid? materialId, DateTime? materialRegistrationDate)
        : base(id, typeName, typeTitle, materialId, materialRegistrationDate)
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
        public Guid? MaterialId { get; }
        public DateTime? MaterialRegistrationDate { get; }
        protected BaseObjectDto(Guid id, string typeName, string typeTitle, Guid? materialId, DateTime? materialRegistrationDate)
        {
            Id = id;
            TypeName = typeName;
            TypeTitle = typeTitle;
            MaterialId = materialId;
            MaterialRegistrationDate = materialRegistrationDate;
        }
    }
}