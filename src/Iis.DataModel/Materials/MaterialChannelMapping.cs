using Iis.DataModel.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    public class MaterialChannelMapping: BaseEntity
    {
        public string ChannelName { get; set; }
        public Guid RoleId { get; set; }
        public RoleEntity Role { get; set; }
    }
}
