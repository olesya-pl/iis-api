using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyManager.DuplicateSearch
{
    public class DuplicateSearchParameter
    {
        public string Url { get; }
        public string EntityTypeName { get; }
        public List<string> DotNames { get; }

        public DuplicateSearchParameter(string param, string url)
        {
            Url = url;
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
