using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.ExtendedData
{
    public class ExtNode
    {
        public string Id { get; set; }
        public string NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public string AttributeValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ExtNode> Children { get; set; } = new List<ExtNode>();
        public bool IsAttribute => AttributeValue != null && Children.Count == 0;
    }
}
