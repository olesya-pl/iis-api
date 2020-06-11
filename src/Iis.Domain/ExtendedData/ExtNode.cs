using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.ExtendedData
{
    public class ExtNode: IExtNode
    {
        public string Id { get; set; }
        public string NodeTypeId { get; set; }
        public string NodeTypeName { get; set; }
        public string NodeTypeTitle { get; set; }
        public string EntityTypeName { get; set; }
        public object AttributeValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IReadOnlyList<IExtNode> Children { get; set; } = new List<ExtNode>();
        public bool IsAttribute => AttributeValue != null && Children.Count == 0;
    }
}
