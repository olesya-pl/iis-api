using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IAttributeType
    {
        Guid Id { get; }
        ScalarType ScalarType { get; set; }
        string GetDefaultValue();
    }
}
