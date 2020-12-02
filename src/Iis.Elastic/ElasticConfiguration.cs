using System;
using System.Text;
using System.Collections.Generic;

namespace Iis.Elastic
{
    public class ElasticConfiguration
    {
        public const string DEFAULT_URI = @"http://localhost:9200";
        public string Uri { get; set; } = DEFAULT_URI;
        public string IndexPreffix { get; set; }
        public int TotalFieldsLimit { get; set; } = 4096;
        public const string DefaultTermVector = "with_positions_offsets";
        public static IEnumerable<string> DefaultDateFormats { get; } = new[] { "date_optional_time", "dd.MM.yyyy, HH:mm:ss", "dd.MM.yyyy", "dd,MM,yyyy", "yyyy.MM.dd", "yyyy,MM,dd", "HH:mm:ss" };
    }
}
