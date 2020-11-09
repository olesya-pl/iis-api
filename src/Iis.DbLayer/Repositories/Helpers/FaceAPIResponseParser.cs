using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories.Helpers
{
    public static class FaceAPIResponseParser
    {
        private const string ImageVectorResultPropery = "result";
        private const string ImageVectorEncodingProperty = "encoding";
        public static decimal[] GetEncoding(string content)
        {
            if(string.IsNullOrWhiteSpace(content)) return null;

            var json = JToken.Parse(content);

            if(json.GetType() == typeof(JArray)) return JsonConvert.DeserializeObject<decimal[]>(content);

            var resultProperty = json[ImageVectorResultPropery];

            if(resultProperty is null) return null;

            var encodings = resultProperty.Children()[ImageVectorEncodingProperty];

            if(!encodings.Any()) return null;

            content = encodings.First().ToString();

            if(string.IsNullOrWhiteSpace(content)) return null;

            var vector = JsonConvert.DeserializeObject<decimal[]>(content);

            if(!vector.Any()) return null;

            return vector;
        }
        public static decimal[] EmptyImageVector => Array.Empty<decimal>();
    }
}
