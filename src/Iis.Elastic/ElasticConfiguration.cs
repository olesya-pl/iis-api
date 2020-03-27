using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Elastic
{
    public class ElasticConfiguration
    {
        public const string DEFAULT_URI = @"http://localhost:9200";
        public string Uri { get; set; } = DEFAULT_URI;
        public string IndexPreffix { get; set; }
    }
}
