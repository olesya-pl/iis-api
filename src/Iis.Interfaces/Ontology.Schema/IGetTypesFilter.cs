using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IGetTypesFilter
    {
        string Name { get; }
        IEnumerable<Kind> Kinds { get; }
    }
}
