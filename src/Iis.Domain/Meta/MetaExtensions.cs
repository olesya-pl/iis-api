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

        public static TMeta CreateMeta<TMeta>(JObject jo, JsonConverter typeConverter) where TMeta : IMeta
        {
            var js = CreateSerializer();
            js.Converters.Add(typeConverter);
            return jo.ToObject<TMeta>(js);
        }

        public static TMeta CreateMeta<TMeta>(INodeTypeModel type, JsonConverter typeConverter) where TMeta : IMeta
        {
            var js = CreateSerializer();
            js.Converters.Add(typeConverter);
            return type.GetFullMeta().ToObject<TMeta>(js);
        }

        public static IMeta CreateMeta(this INodeTypeModel type)
        {
            if (type?.MetaSource == null)
                return null;
            if (type is IAttributeTypeModel attributeType)
                return attributeType.CreateMeta();
            if (type is IEmbeddingRelationTypeModel relationType)
                return relationType.CreateMeta();
            if (type is EntityType entityType)
                return entityType.CreateMeta();
            throw new ArgumentException(nameof(type));
        }

        public static EntityMeta CreateMeta(this EntityType type) =>
            CreateMeta<EntityMeta>(type, new MetaConverter<EntityMeta>(null));

        public static AttributeMeta CreateMeta(this IAttributeTypeModel type) =>
            CreateMeta<AttributeMeta>(type, new MetaConverter<AttributeMeta>(type.ScalarTypeEnum));

        public static RelationMetaBase CreateMeta(this IEmbeddingRelationTypeModel type) =>
            type.IsAttributeType ? (RelationMetaBase) CreateAttributeRelationMeta(type) : CreateEntityRelationMeta(type);

        public static AttributeRelationMeta CreateAttributeRelationMeta(this IEmbeddingRelationTypeModel type)
        {
            if (!type.IsAttributeType) throw new ArgumentException(nameof(type));
            var converter = new MetaConverter<AttributeRelationMeta>(type.IAttributeTypeModel.ScalarTypeEnum);
            return CreateMeta<AttributeRelationMeta>(type, converter);
        }

        public static EntityRelationMeta CreateEntityRelationMeta(this IEmbeddingRelationTypeModel type)
        {
            if (!type.IsEntityType) throw new ArgumentException(nameof(type));
            var converter = new MetaConverter<EntityRelationMeta>(null);
            return CreateMeta<EntityRelationMeta>(type, converter);
        }

        public static bool IsComputed(this IEmbeddingRelationTypeModel type) => type.GetComputed() != null;

        public static string GetComputed(this IEmbeddingRelationTypeModel type)
            => (type.Meta as AttributeRelationMeta)?.Formula;

        public static bool HasInversed(this IEmbeddingRelationTypeModel type) => type.GetInversed() != null;

        public static InversedRelationMeta GetInversed(this IEmbeddingRelationTypeModel type)
            => (type.Meta as EntityRelationMeta)?.Inversed;

        public static JObject Serialize(this IMeta meta) => JObject.FromObject(meta, CreateSerializer());

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
