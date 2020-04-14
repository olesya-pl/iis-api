using System;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using Iis.DataModel.Reports;

namespace Iis.DataModel
{
    public class MLResponseEntity : BaseEntity
    {
        public Guid MaterialId { get; set; }
        public String MLHandlerName { get; set; }
        public String OriginalResponse { get; set; }
    }
}
