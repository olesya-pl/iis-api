﻿using Iis.Interfaces.Elastic;
using System.Collections.Generic;

namespace Iis.Domain.Elastic
{
    public class IisElasticSearchParams : IIisElasticSearchParams
    {
        public IReadOnlyList<IIisElasticField> SearchFields { get; set; } = new List<IisElasticField>();
        public IEnumerable<string> BaseIndexNames {get; set; } = new List<string>();
        public IEnumerable<string> ResultFields { get; set; } = new List<string> { "*" };
        public string Query {get;set;}
        public int From { get; set; }
        public int Size { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public bool IsLenient { get; set; } = true;

    }
}
