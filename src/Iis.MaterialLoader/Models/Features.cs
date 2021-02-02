using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.MaterialLoader.Models
{
    public class Features
    {
        public IEnumerable<Node> Nodes { get; set; }
        public JObject Metadata { get; set; }
    }
}