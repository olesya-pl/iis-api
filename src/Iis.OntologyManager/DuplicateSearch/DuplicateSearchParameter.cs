using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Iis.OntologyManager.DuplicateSearch
{
    public class DuplicateSearchParameter
    {
        public string Url { get; }
        public string EntityTypeName { get; }
        public List<string> DotNames { get; }
        public List<string> DistinctNames { get; set; }

        public DuplicateSearchParameter(string param, string url)
        {
            Url = url;
            var regex = new Regex(@"(\S+):([^(]+)(\(([^)]+)\))?");
            var match = regex.Match(param);
            if (!match.Success) throw new ArgumentException();
            EntityTypeName = match.Groups[1].ToString();
            DotNames = match.Groups[2].ToString()
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            DistinctNames = match.Groups[4].ToString()
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

        }
    }
}
