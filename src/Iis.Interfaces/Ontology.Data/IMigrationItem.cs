using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IMigrationItem
    {
        string SourceDotName { get; }
        string TargetDotName { get; }
        IMigrationItemOptions Options { get; }
    }
}
