using System;
using System.Collections.Generic;

namespace Iis.DataModel.Materials
{
    public class MaterialEntity : BaseEntity
    {
        public Guid? ParentId { get; set; }
        public virtual MaterialEntity Parent { get; set; }

        public Guid? FileId { get; set; }
        public virtual FileEntity File { get; set; }

        public string Metadata { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<MaterialEntity> Children { get; set; }

        public virtual ICollection<MaterialInfoEntity> MaterialInfos { get; set; }
    }
}
