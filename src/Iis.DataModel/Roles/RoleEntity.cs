using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class RoleEntity: BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsArchived { get; set; } = false;
    }
}
