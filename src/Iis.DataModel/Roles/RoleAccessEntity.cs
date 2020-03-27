using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class RoleAccessEntity: BaseEntity
    {
        public Guid RoleId { get; set; }
        public Guid AccessObjectId { get; set; }
        public string Operations { get; set; }
        public RoleEntity Role { get; set; }
        public AccessObjectEntity AccessObject { get; set; }
    }
}
