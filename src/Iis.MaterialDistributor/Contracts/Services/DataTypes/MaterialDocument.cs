using System;
namespace Iis.MaterialDistributor.Contracts.Services
{
    public class MaterialDocument
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int VariableCoefficient { get; set; }
    }
}
