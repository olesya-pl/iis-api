using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology.Meta
{
    public static class MetaExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
            {MissingMemberHandling = MissingMemberHandling.Error, NullValueHandling = NullValueHandling.Ignore};
        public static JsonSerializer CreateSerializer() => JsonSerializer.Create(SerializerSettings);

        public static TMeta CreateMeta<TMeta>(Type type, JsonConverter typeConverter) where TMeta : IMeta
        {
            var js = CreateSerializer();
            js.Converters.Add(typeConverter);
            return type.GetFullMeta().ToObject<TMeta>(js);
        }

        public static IMeta CreateMeta(this Type type)
        {
            if (type?.MetaSource == null)
                return null;
            if (type is AttributeType attributeType)
                return attributeType.CreateMeta();
            if (type is EmbeddingRelationType relationType)
                return relationType.CreateMeta();
            if (type is EntityType entityType)
                return entityType.CreateMeta();
            throw new ArgumentException(nameof(type));
        }

        public static EntityMeta CreateMeta(this EntityType type) =>
            CreateMeta<EntityMeta>(type, new MetaConverter<EntityMeta>(null));

        public static AttributeMeta CreateMeta(this AttributeType type) =>
            CreateMeta<AttributeMeta>(type, new MetaConverter<AttributeMeta>(type.ScalarTypeEnum));

        public static RelationMetaBase CreateMeta(this EmbeddingRelationType type) =>
            type.IsAttributeType ? (RelationMetaBase) CreateAttributeRelationMeta(type) : CreateEntityRelationMeta(type);

        public static AttributeRelationMeta CreateAttributeRelationMeta(this EmbeddingRelationType type)
        {
            if (!type.IsAttributeType) throw new ArgumentException(nameof(type));
            var converter = new MetaConverter<AttributeRelationMeta>(type.AttributeType.ScalarTypeEnum);
            return CreateMeta<AttributeRelationMeta>(type, converter);
        }

        public static EntityRelationMeta CreateEntityRelationMeta(this EmbeddingRelationType type)
        {
            if (!type.IsEntityType) throw new ArgumentException(nameof(type));
            var converter = new MetaConverter<EntityRelationMeta>(null);
            return CreateMeta<EntityRelationMeta>(type, converter);
        }

        public static bool IsComputed(this EmbeddingRelationType type) => type.GetComputed() != null;

        public static string GetComputed(this EmbeddingRelationType type)
            => (type.Meta as AttributeRelationMeta)?.Formula;

        public static bool HasInversed(this EmbeddingRelationType type) => type.GetInversed() != null;

        public static InversedRelationMeta GetInversed(this EmbeddingRelationType type)
            => (type.Meta as EntityRelationMeta)?.Inversed;

        public static JObject Serialize(this IMeta meta) => JObject.FromObject(meta, CreateSerializer());

        // ugly quick solution to validate existing ontology meta
        public static void ValidateMeta(this IEnumerable<Type> ontologyTypes)
        {
            foreach (var type in ontologyTypes)
                ValidateMeta(type);
        }

        public static void ValidateMeta(Type type)
        {
            try
            {
                var meta = type.CreateMeta();
                foreach (var node in type.RelatedTypes)
                    meta = node.CreateMeta();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{type.Name} - {type.GetType()}");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(type.MetaSource);
            }
        }

        public static JObject GetFullMeta(this Type type)
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
