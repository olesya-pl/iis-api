using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var sb = new StringBuilder();
            
            foreach (var item in documentDictionary)
            {
                var documentJson = JsonConvert.SerializeObject(item.Value);

                sb.Append($"{{\"index\":{{\"_id\":\"{item.Key:N}\"}}}}\n{documentJson}{Environment.NewLine}");
            }
            
            return sb.ToString();
        }
    }
}