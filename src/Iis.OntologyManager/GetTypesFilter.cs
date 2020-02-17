using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager
{
    public class GetTypesFilter: IGetTypesFilter
    {
        public string Name { get; set; }
        public IEnumerable<Kind> Kinds { get; set; }
    }
}
