using Iis.DataModel.Materials;
using Iis.DataModel.Roles;
using System.Collections.Generic;
using Iis.Interfaces.Enums;

namespace Iis.DataModel
{
    public class UserEntity: BaseEntity
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string Comment { get; set; }
        public string PasswordHash { get; set; }
        public string UserNameActiveDirectory { get; set; }
        public bool IsBlocked { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public List<UserRoleEntity> UserRoles { get; set; }
        public List<MaterialEntity> Materials { get; internal set; }
    }
}
