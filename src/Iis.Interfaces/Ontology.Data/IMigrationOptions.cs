using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IMigrationOptions
    {
        bool SaveNewObjects { get; }
        bool DeleteOldObjects { get; }
    }
}
