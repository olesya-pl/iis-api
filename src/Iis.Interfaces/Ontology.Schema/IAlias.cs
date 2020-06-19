using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface IAlias
    {
        string DotName { get; }
        string Value { get; }
    }
}
