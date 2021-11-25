using System;

namespace Iis.DataModel.Roles
{
    public class RoleActiveDirectoryGroupEntity : BaseEntity
    {
        public Guid RoleId { get; set; }

        public RoleEntity Role { get; set; }

        public Guid GroupId { get; set; }
    }
}