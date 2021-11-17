using System;
using Iis.Interfaces.Materials;

namespace Iis.DataModel.Materials
{
    public class MaterialSignEntity : BaseEntity, IMaterialSign
    {
        public Guid MaterialSignTypeId { get; set; }
        public string ShortTitle { get; set; }
        public string Title { get; set; }
        public int OrderNumber { get; set; }
        public MaterialSignTypeEntity MaterialSignType { get; set; }
    }
}
