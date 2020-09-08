using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IAttributeBase
    {
        Guid Id { get; }
        string Value { get; }
    }
}
