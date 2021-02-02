using Newtonsoft.Json.Linq;

namespace Iis.MaterialLoader.Models
{
    public class Node
    {
        public string Relation { get; set; }
        public string Type { get; set; } // todo confluence
        public string Value { get; set; }
        public JObject Original { get; set; }
        public UpdateFieldData UpdateField { get; set; }
    }
}