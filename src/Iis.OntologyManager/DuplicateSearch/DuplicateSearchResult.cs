using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.DuplicateSearch
{
    public class DuplicateSearchResult
    {
        public List<DuplicateSearchResultItem> Items { get; } = new List<DuplicateSearchResultItem>();
    }
}