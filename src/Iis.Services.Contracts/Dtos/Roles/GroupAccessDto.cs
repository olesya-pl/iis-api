using System;
using System.Collections.Generic;

namespace Iis.Services.Contracts.Dtos.Roles
{
    public class GroupAccessDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<RoleDto> Roles { get; set; }
    }
}