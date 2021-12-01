using System.Collections.Generic;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResResult
    {
        public IReadOnlyList<ResLink> Links { get; set; }
        public IReadOnlyList<ResNode> Nodes { get; set; }
    }
}
