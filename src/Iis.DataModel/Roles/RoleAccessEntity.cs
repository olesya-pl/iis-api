using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class RoleAccessEntity: BaseEntity
    {
        public Guid RoleId { get; set; }
        public AccessKind Kind { get; set; }
        public string Operations { get; set; }
        public RoleEntity Role { get; set; }
    }
}
