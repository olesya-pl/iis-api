using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IMigrationItemOptions
    {
        string IgnoreIfFieldsAreNotEmpty { get; }
        bool Ignore { get; }
    }
}
