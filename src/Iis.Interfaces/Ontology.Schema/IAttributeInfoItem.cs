using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IAttributeInfoItem
    {
        public string DotName { get; }
        public ScalarType ScalarType { get; }
    }
}
