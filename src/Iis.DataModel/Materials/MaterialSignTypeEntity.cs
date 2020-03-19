using Iis.Interfaces.Materials;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Materials
{
    public class MaterialSignTypeEntity: BaseEntity, IMaterialSignType
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public ICollection<MaterialSignEntity> MaterialSigns { get; set; }
    }
}
