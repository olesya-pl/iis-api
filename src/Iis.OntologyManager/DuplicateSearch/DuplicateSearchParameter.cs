using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.DuplicateSearch
{
    public class DuplicateSearchParameter
    {
        public string Url { get; }
        public List<DuplicateSearchEntityParameter> EntityParameters { get; } = new List<DuplicateSearchEntityParameter>();
        public DuplicateSearchParameter(string param, string url)
        {
            Url = url;
            foreach (var entityParamStr in param.Split(';'))
            {
                EntityParameters.Add(new DuplicateSearchEntityParameter(entityParamStr));
            }
        }
    }
}
