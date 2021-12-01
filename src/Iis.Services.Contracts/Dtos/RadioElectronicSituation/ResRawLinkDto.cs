using Iis.DataModel.Materials;
using System;

namespace Iis.Services.Contracts.Dtos.RadioElectronicSituation
{
    public class ResRawLinkDto
    {
        public Guid MaterialId { get; set; }
        public Guid NodeId { get; set; }
        public MaterialNodeLinkType NodeLinkType { get; set; }
    }
}
