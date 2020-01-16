using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.Materials
{
    public class MaterialAddedEvent
    {
        public Guid FileId;
        public Guid MaterialId;
        public Guid EntityId;
        public List<IIS.Core.GraphQL.Materials.Node> Nodes { get; set; }
    }
}
