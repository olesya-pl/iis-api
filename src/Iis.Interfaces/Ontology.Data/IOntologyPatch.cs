using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IOntologyPatch
    {
        IOntologyPatchItem Create { get; }
        IOntologyPatchItem Update { get; }
        void Clear();
    }
}
