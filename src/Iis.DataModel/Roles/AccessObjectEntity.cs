using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class AccessObjectEntity: BaseEntity
    {
        [Required]
        public string Title { get; set; }
        public AccessKind Kind { get; set; }
        public AccessCategory Category { get; set; }
        public bool CreateAllowed { get; set; }
        public bool ReadAllowed { get; set; }
        public bool UpdateAllowed { get; set; }
        public bool DeleteAllowed { get; set; }
        public List<RoleAccessEntity> RoleAccessEntities { get; set; }
    }
}
