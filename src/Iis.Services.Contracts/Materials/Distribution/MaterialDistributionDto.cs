using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class MaterialDistributionDto
    {
        public Guid Id { get; }
        public string Source { get; }
        public string RoleName { get; }
        public MaterialDistributionDto() { }
        public MaterialDistributionDto(Guid id, string source, string roleName)
        {
            Id = id;
            Source = source;
            RoleName = roleName;
        }
    }
}
