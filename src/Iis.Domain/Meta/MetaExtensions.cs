using System;
using System.Linq;
using Iis.Interfaces.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.Domain.Meta
{
    public static class MetaExtensions
    {
        public static bool IsComputed(this IEmbeddingRelationTypeModel type) => type.GetComputed() != null;

        public static string GetComputed(this IEmbeddingRelationTypeModel type)
            => type.Meta?.Formula;
    }
}
