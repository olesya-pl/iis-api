using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class RoleEntity: BaseEntity
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        
        public List<RoleAccessEntity> RoleAccessEntities { get; set; }
        public List<UserRoleEntity> UserRoles { get; set; }
    }
}
