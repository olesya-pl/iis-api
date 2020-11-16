using System;
using System.Linq;
using Iis.Interfaces.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.Domain.Meta
{
    public static class MetaExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
            {MissingMemberHandling = MissingMemberHandling.Error, NullValueHandling = NullValueHandling.Ignore};
        public static JsonSerializer CreateSerializer() => JsonSerializer.Create(SerializerSettings);

        public static bool IsComputed(this IEmbeddingRelationTypeModel type) => type.GetComputed() != null;

        public static string GetComputed(this IEmbeddingRelationTypeModel type)
            => (type.Meta as IAttributeRelationMeta)?.Formula;

        public static JObject GetFullMeta(this INodeTypeModel type)
        {
            var settings = new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            };
            var meta = new JObject();
            var hierarchy = type.AllParents.Concat(new[] {type});
            foreach (var t in hierarchy)
                if (t.MetaSource != null)
                    meta.Merge(t.MetaSource, settings);
            return meta;
        }
    }
}
