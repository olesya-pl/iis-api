using System;
using System.Collections.Generic;

namespace Iis.EventMaterialAutoAssignment
{
    public class AssignmentConfig
    {
        public Guid Id { get; set; }
        public string AccessLevel { get; set; }
        public string Component { get; set; }
        public string EventType { get; set; }
        public string Importance { get; set; }
        public string Name { get; set; }
        public string RelatesToCountry { get; set; }
        public string State { get; set; }
        public List<AssignmentConfigKeyword> Keywords { get; set; }
    }
}
