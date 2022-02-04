using Iis.Interfaces.SecurityLevels;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public class MaterialDistributionInfo
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int VariableCoefficient { get; set; }
        [JsonIgnore]
        public IReadOnlyList<ISecurityLevel> SecurityLevels { get; set; } = new List<ISecurityLevel>();
        [JsonIgnore]
        public decimal FinalRating { get; set; }
    }
}
