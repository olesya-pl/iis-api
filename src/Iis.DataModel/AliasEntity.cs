using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel
{
    public class AliasEntity: BaseEntity
    {
        public string DotName { get; set; }
        public string Value { get; set; }
    }
}
