using System;

namespace Iis.DataModel.Roles
{
    public class UserRoleEntity : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public short? MaterialChannelCoefficient { get; set; }
        public UserEntity User { get; set; }
        public RoleEntity Role { get; set; }

        public static UserRoleEntity CreateFrom(Guid userId, Guid roleId) =>
            new UserRoleEntity { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId };
    }
}