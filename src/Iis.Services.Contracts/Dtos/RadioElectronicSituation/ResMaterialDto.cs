using System;

namespace Iis.Services.Contracts.Dtos.RadioElectronicSituation
{
    public class ResMaterialDto
    {
        public Guid Id { get; }
        public string Type { get; }
        public string Source { get; }
        public string Title { get; }
        public DateTime CreatedDate { get; }
        public DateTime? RegistrationDate { get; }
        public ResMaterialDto(Guid id, string type, string source, string title, DateTime createdDate, DateTime? registrationDate)
        {
            Id = id;
            Type = type;
            Source = source;
            Title = title;
            CreatedDate = createdDate;
            RegistrationDate = registrationDate;
        }
    }
}
