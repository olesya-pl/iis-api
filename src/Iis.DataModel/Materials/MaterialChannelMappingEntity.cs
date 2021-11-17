using System;
using Iis.DataModel.Roles;

namespace Iis.DataModel.Materials
{
    public class MaterialChannelMappingEntity : BaseEntity
    {
        public string ChannelName { get; set; }
        public Guid RoleId { get; set; }
        public RoleEntity Role { get; set; }
    }
}
