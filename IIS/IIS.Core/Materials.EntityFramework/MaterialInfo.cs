using System;
using System.Collections.Generic;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialInfo
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public string Data { get; set; }
        public string Source { get; set; }
        public string SourceType { get; set; }
        public string SourceVersion { get; set; }

        public virtual Material Material { get; set; }
        public virtual ICollection<MaterialFeature> Features { get; set; }
    }
}
