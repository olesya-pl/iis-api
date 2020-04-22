using System;
using Iis.Interfaces.Materials;

namespace Iis.DataModel.Materials
{
    public class MLResponseEntity : BaseEntity, IMLResponseEntity
    {
        public Guid MaterialId { get; set; }
        public String MLHandlerName { get; set; }
        public String OriginalResponse { get; set; }
    }
}
