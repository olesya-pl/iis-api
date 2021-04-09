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
        public bool CreateGranted { get; set; }
        public bool ReadGranted { get; set; }
        public bool UpdateGranted { get; set; }
        public bool SearchGranted { get; set; }
        public bool CommentingGranted { get; set; }
        public bool AccessLevelUpdateGranted { get; set; }
        public RoleEntity Role { get; set; }
        public AccessObjectEntity AccessObject { get; set; }
    }
}
