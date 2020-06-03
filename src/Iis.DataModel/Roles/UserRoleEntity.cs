using System;

namespace Iis.DataModel.Roles
{
    public class UserRoleEntity: BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public UserEntity User { get; set; }
        public RoleEntity Role { get; set; }
    }
}
