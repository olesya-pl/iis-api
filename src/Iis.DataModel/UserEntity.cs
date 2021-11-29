using System;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using Iis.DataModel.Roles;
using Iis.Interfaces.Users;

namespace Iis.DataModel
{
    public class UserEntity : BaseEntity
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string Comment { get; set; }
        public string PasswordHash { get; set; }
        [Obsolete("Deprecated in favor of Source")]
        public string UserNameActiveDirectory { get; set; }
        public bool IsBlocked { get; set; }
        public int AccessLevel { get; set; }
        public UserSource Source { get; set; }
        public List<UserRoleEntity> UserRoles { get; set; }
        public List<MaterialAssigneeEntity> MaterialAssignees { get; internal set; }
    }
}
