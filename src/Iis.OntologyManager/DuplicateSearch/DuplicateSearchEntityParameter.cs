using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyManager.DuplicateSearch
{
    public class DuplicateSearchEntityParameter
    {
        public string EntityTypeName { get; }
        public List<string> DotNames { get; }
        
        public DuplicateSearchEntityParameter(string param)
        {
            var semiColonIndex = param.IndexOf(':');
            EntityTypeName = param.Substring(0, semiColonIndex).Trim();
            DotNames = param
                .Substring(semiColonIndex + 1)
                .Split(',')
                .Select(s => s.Trim())
                .ToList();
        }
    }
}
