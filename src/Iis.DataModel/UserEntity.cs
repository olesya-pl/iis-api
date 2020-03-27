using Iis.DataModel.Roles;
using System;
using System.Collections.Generic;

namespace Iis.DataModel
{
    public class UserEntity: BaseEntity
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public bool IsBlocked { get; set; }
        public List<UserRoleEntity> Roles { get; set; }
    }
}
