using System;
using System.Diagnostics;

namespace Iis.Ontology.DataRead.Raw
{
    [DebuggerDisplay("{Kind} {Name}")]
    public sealed class OntologyItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public ItemKind Kind { get; set; }
    }
}
