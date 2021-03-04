using System.Collections.Generic;
using Iis.Elastic.Dictionaries;
namespace Iis.Elastic
{
    public class ElasticConfiguration
    {
        public const string DEFAULT_URI = @"http://localhost:9200";
        public string Uri { get; set; } = DEFAULT_URI;
        public string IndexPreffix { get; set; }
        public int TotalFieldsLimit { get; set; } = 4096;
        public string DefaultLogin { get; set; }
        public string DefaultPassword { get; set; }
        public int ScrollDurationMinutes { get; set; }
        public const TextTermVectorsEnum DefaultTermVector = TextTermVectorsEnum.WithPositionsOffsets;
        public static IReadOnlyCollection<string> DefaultDateFormats { get; } = new[] { "date_optional_time", "dd.MM.yyyy, HH:mm:ss", "dd.MM.yyyy", "dd,MM,yyyy", "yyyy.MM.dd", "yyyy,MM,dd", "HH:mm:ss" };
    }
}
