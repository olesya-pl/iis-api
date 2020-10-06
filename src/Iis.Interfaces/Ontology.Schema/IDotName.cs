using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IDotName
    {
        string Value { get; }
        IReadOnlyList<string> Parts { get; }
        IDotName Concat(IDotName another);
    }
}
