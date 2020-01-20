using System;
using System.Collections.Generic;
using Iis.Domain;

namespace IIS.Core.Ontology
{
    public class NodeFilter
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Suggestion { get; set; }
        public List<Tuple<EmbeddingRelationType, string>> SearchCriteria { get; set; } = new List<Tuple<EmbeddingRelationType, string>>();
        public bool AnyOfCriteria { get; set; }
        public bool ExactMatch { get; set; }
    }
}
