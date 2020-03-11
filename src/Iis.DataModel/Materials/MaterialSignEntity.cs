using Iis.Interfaces.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    public class MaterialSignEntity: BaseEntity, IMaterialSign
    {
        public Guid MaterialSignTypeId { get; set; }
        public string ShortTitle { get; set; }
        public string Title { get; set; }
        public int OrderNumber { get; set; }
        public MaterialSignTypeEntity MaterialSignType { get; set; }
    }
}
