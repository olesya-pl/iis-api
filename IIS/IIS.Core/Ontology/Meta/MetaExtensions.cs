using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace IIS.Core.Ontology.Meta
{
    public static class MetaExtensions
    {
        public static JsonSerializer CreateSerializer() =>
            new JsonSerializer {MissingMemberHandling = MissingMemberHandling.Error, NullValueHandling = NullValueHandling.Ignore};
        
        public static ITypeMeta CreateMeta(this Type type)
        {
            if (type?.Meta == null)
                return null;
            if (type is AttributeType attributeType)
                return attributeType.CreateMeta();
            if (type is EmbeddingRelationType relationType)
                return relationType.CreateMeta();
            if (type is EntityType entityType)
                return entityType.CreateMeta();
            var js = CreateSerializer();
            js.Converters.Add(new TypeMetaConverter(null));
            return type.Meta?.ToObject<ITypeMeta>(js);
        }

        public static ITypeMeta CreateMeta(this EntityType type)
        {
            var js = CreateSerializer();
            js.Converters.Add(new TypeMetaConverter(null));
            var meta = new TypeMeta();
            var hierarchy = type.AllParents.Concat(new[] {type});
            foreach (var t in hierarchy)
            {
                if (t.Meta == null) continue;
                using (var reader = t.Meta.CreateReader())
                    js.Populate(reader, meta);
            }

            return meta;
        }

        public static ITypeMeta CreateMeta(this EmbeddingRelationType type)
        {
            var js = CreateSerializer();
            js.Converters.Add(type.IsAttributeType
                ? new RelationTypeMetaConverter(type.AttributeType.ScalarTypeEnum)
                : new RelationTypeMetaConverter(null));
            return type.Meta?.ToObject<ITypeMeta>(js);
        }

        public static ITypeMeta CreateMeta(this AttributeType type)
        {
            var js = CreateSerializer();
            js.Converters.Add(new TypeMetaConverter(type.ScalarTypeEnum));
            return type.Meta?.ToObject<ITypeMeta>(js);
        }

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
                foreach (var node in type.Nodes)
                    meta = node.CreateMeta();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{type.Name} - {type.GetType()}");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(type.Meta);
            }
        }
    }
}