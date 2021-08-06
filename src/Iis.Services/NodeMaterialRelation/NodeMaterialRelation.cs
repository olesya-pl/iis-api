using Iis.DataModel.Materials;
using System;

namespace Iis.Services
{
    public class NodeMaterialRelation
    {
        public Guid NodeId { get; set; }
        public Guid MaterialId { get; set; }
        public MaterialNodeLinkType NodeLinkType { get; set; }
    }
}
