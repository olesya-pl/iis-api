using System;
namespace Iis.Domain.Materials
{
    public class MaterialRelation
    {
        public Guid NodeId { get; set; }
        public EntityMaterialRelation NodeRelation { get; set; }
    }
}