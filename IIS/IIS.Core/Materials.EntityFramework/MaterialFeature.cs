using System;
using IIS.Core.Ontology.EntityFramework.Context;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialFeature
    {
        public Guid Id { get; set; }
        public Guid MaterialInfoId { get; set; }
        public string Relation { get; set; }
        public string Value { get; set; }
        public Guid NodeId { get; set; }

        public virtual MaterialInfo Info { get; set; }
        public virtual Node Node { get; set; }
    }
}
