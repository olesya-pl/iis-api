using System;
using System.Collections.Generic;

namespace Iis.DataModel.Materials
{
    public class MaterialInfoEntity : BaseEntity
    {
        public Guid MaterialId { get; set; }
        public virtual MaterialEntity Material { get; set; }

        public string Data { get; set; }
        public string Source { get; set; }
        public string SourceType { get; set; }
        public string SourceVersion { get; set; }

        public virtual ICollection<MaterialFeatureEntity> MaterialFeatures { get; set; }
    }
}
