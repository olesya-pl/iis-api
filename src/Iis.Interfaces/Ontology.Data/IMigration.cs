using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IMigration
    {
        string Title { get; }
        IReadOnlyList<IMigrationEntity> GetItems();
        IReadOnlyList<IMigrationReference> GetReferences();
    }
}
