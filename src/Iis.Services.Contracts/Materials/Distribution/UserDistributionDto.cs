using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services.Contracts.Materials.Distribution
{
    public class UserDistributionDto
    {
        public Guid Id { get; }
        public IReadOnlyList<string> RoleNames { get; }
        public UserDistributionDto() { }
        public UserDistributionDto(Guid id, IEnumerable<string> roleNames)
        {
            Id = id;
            RoleNames = roleNames.ToList();
        }
    }
}
