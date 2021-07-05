using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Domain.Graph
{
    public class GraphLink
    {
        public Guid Id { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }
        public JObject Extra { get; set; }
    }
}