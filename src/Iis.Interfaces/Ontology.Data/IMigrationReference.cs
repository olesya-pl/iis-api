using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IMigrationReference
    {
        string EntityName { get; }
        string SourceDotName { get; }
        string TargetDotName { get; }
    }
}
