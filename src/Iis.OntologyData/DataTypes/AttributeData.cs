using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class AttributeData: IAttribute
    {
        public Guid Id { get; set; }
        public string Value { get; set; }

        internal NodeData _node;
        public INode Node => _node;
    }
}
