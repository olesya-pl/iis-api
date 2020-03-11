using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialSignFull: MaterialSign
    {
        public Guid MaterialSignTypeId { get; set; }
        public int OrderNumber { get; set; }
        public string TypeName { get; set; }
        public string TypeTitle { get; set; }
    }
}
