using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IAttributeInfoItem
    {
        string DotName { get; }
        ScalarType ScalarType { get; }
    }
}
