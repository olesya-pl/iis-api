using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    public class MaterialSecurityLevelEntity : BaseEntity
    {
        public Guid MaterialId { get; set; }
        public MaterialEntity Material { get; set; }
        public int SecurityLevelIndex { get; set; }
    }
}
