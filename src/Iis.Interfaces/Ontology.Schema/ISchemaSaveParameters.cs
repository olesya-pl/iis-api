using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Schema
{
    public interface ISchemaSaveParameters
    {
        bool Create { get; }
        bool Update { get; }
        bool Delete { get; }
        bool Aliases { get; }
    }
}
