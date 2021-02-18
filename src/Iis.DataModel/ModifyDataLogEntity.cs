using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel
{
    public class ModifyDataLogEntity: BaseEntity
    {
        public string Name { get; set; }
        public DateTime RunDate { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
