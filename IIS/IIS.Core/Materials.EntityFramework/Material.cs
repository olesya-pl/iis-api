using System;
using System.Collections.Generic;
using IIS.Core.Files.EntityFramework;

namespace IIS.Core.Materials.EntityFramework
{
    public class Material
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? FileId { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }

        public virtual Material Parent { get; set; }
        public virtual ICollection<Material> Children { get; set; }
        public virtual File File { get; set; }
        public virtual ICollection<MaterialInfo> Infos { get; set; }
    }
}
