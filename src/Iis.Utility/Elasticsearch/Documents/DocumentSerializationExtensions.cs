using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Utility.Elasticsearch.Documents
{
    public static class DocumentSerializationExtensions
    {
        public static string ConvertToJson<TDocument>(this IReadOnlyDictionary<Guid, TDocument> documentDictionary)
        {
            if (documentDictionary is null)
                throw new ArgumentNullException(nameof(documentDictionary));
            if (documentDictionary.Count == 0)
                throw new ArgumentException("Dictionary is empty", nameof(documentDictionary));
            if (documentDictionary.Any(_ => _.Value is null))
                throw new ArgumentNullException(nameof(documentDictionary), "Some element is null");

            var values = documentDictionary.Select(_ =>
            {
                string documentJson = JsonConvert.SerializeObject(_.Value);

                return $"{{\"index\":{{\"_id\":\"{_.Key:N}\"}}}}\n{documentJson}{Environment.NewLine}";
            });

            return string.Join(string.Empty, values);
        }
    }
}