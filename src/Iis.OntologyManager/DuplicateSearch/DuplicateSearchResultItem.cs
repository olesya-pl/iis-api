using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyManager.DuplicateSearch
{
    public class DuplicateSearchResultItem
    {
        public int OrderNumber { get; set; }
        public string Value { get; set; }
        public string DotName { get; set; }
        public string Url { get; set; }
        public INode Node { get; set; }
        public int LinksCount => Node.IncomingRelations.Count();
        public override string ToString()
        {
            return $"{DotName}: {Value}";
        }
    }
}
