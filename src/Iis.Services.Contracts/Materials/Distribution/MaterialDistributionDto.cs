using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class MaterialDistributionDto
    {
        public Guid Id { get; }
        public int Priority { get; }
        public string RoleName { get; }
        public MaterialDistributionDto() { }
        public MaterialDistributionDto(Guid id, int priority, string roleName)
        {
            Id = id;
            Priority = priority;
            RoleName = roleName;
        }
    }
}
