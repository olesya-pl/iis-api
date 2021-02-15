﻿using Iis.Interfaces.Elastic;

namespace Iis.Domain
{
    public class ElasticFilter : IElasticNodeFilter
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Suggestion { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public bool IncludeAggregations { get; set; }
    }
}
