using Iis.Interfaces.Roles;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public bool SearchAllowed { get; set; }
        public bool CommentingAllowed { get; set; }
        public bool AccessLevelUpdateAllowed { get; set; }
        public List<RoleAccessEntity> RoleAccessEntities { get; set; }
    }
}
