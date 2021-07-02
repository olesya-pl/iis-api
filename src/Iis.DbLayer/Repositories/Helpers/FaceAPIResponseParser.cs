using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Repositories.Helpers
{
    public static class FaceAPIResponseParser
    {
        private const string ImageVectorResultPropery = "result";
        private const string ImageVectorEncodingProperty = "encoding";
        private static readonly IReadOnlyCollection<decimal[]> EmptyFaceVectorList = Array.Empty<decimal[]>();

        public static decimal[] GetFaceVector(string content)
        {
            return GetFaceVectorList(content).FirstOrDefault();
        }
        public static IReadOnlyCollection<decimal[]> GetFaceVectorList(string content)
        {
            if(string.IsNullOrWhiteSpace(content)) return EmptyFaceVectorList;

            var json = JToken.Parse(content);

            if(json.GetType() == typeof(JArray))
            {
                var vector = JsonConvert.DeserializeObject<decimal[]>(content);

                return new decimal[][] {vector};
            }

            var resultProperty = json[ImageVectorResultPropery];

            if(resultProperty is null) return EmptyFaceVectorList;

            var encodingList = resultProperty.Children()[ImageVectorEncodingProperty];

            if(!encodingList.Any()) return EmptyFaceVectorList;

            return encodingList
                .Select(e => e.ToString())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(JsonConvert.DeserializeObject<decimal[]>)
                .Where(e => e.Length != 0)
                .ToArray();
        }
    }
}
