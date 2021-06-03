using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
namespace Iis.Domain.Graph
{
    public class GraphNode
    {
        public Guid Id { get; set; }
        public JObject Extra {get;set;}
    }
}