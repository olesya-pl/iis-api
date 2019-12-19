using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology.EntityFramework
{
    public class JsonOntologyProvider : IOntologyProvider
    {
        private string _basePath;

        public JsonOntologyProvider(string basePath)
        {
            _basePath = basePath;
        }

        public async Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default)
        {
            var ctx = new LoadingContext();
            _loadRawTypes(ctx);
            var types = _linkTypes(ctx);

            return new Ontology(types);
        }

        private void _loadRawTypes(LoadingContext ctx)
        {
            var files = Directory.GetFiles(_basePath, "*.json", SearchOption.AllDirectories);

            foreach (var path in files)
            {
                var conceptType = Path.GetFileName(Path.GetDirectoryName(path));
                var dataType = _parseToDataType(path);
                var rawType = _mapTypeToRawType(dataType);

                ctx.RawTypes[dataType.Id] = rawType;

                if (!(rawType.OntologyType is RelationType))
                    ctx.TypesByName[dataType.Name] = rawType.OntologyType;
            }
        }

        private Serializer.DataType<JObject> _parseToDataType(string path)
        {
            var content = File.ReadAllText(Path.Combine(_basePath, path), Encoding.UTF8);
            return JsonConvert.DeserializeObject<Serializer.DataType<JObject>>(content);
        }

        private UnSerializedType _mapTypeToRawType(Serializer.DataType<JObject> dataType)
        {
            Type type;

            switch (dataType.ConceptType)
            {
                case "entities":
                    type = new EntityType(dataType.Id, dataType.Name, dataType.Abstract == true);
                    break;
                case "relations":
                    type = RelationType.Build(dataType.Id, dataType.Name, _metaToEmbeddingOptions(dataType.Meta));
                    break;
                case "attributes":
                    ScalarType attrDataType;
                    Enum.TryParse(dataType.Type, out attrDataType);
                    type = new AttributeType(dataType.Id, dataType.Name, attrDataType);
                    break;
                default:
                    throw new ArgumentException($"Unknown concept type {dataType.ConceptType}");
            }

            type.Title = dataType.Title;
            type.MetaSource = dataType.Meta;
            type.CreatedAt = DateTime.UtcNow;
            type.UpdatedAt = DateTime.UtcNow;

            return new UnSerializedType { OntologyType = type, DataType = dataType };
        }

        private EmbeddingOptions _metaToEmbeddingOptions(JObject meta)
        {
            if (meta == null)
                return EmbeddingOptions.Optional;

            if ((bool)meta["multiple"])
                return EmbeddingOptions.Multiple;

            if (meta["validation"] != null && (bool)meta["validation"]["required"])
                return EmbeddingOptions.Required;

            return EmbeddingOptions.Optional;
        }

        private IEnumerable<Type> _linkTypes(LoadingContext ctx)
        {
            var types = new List<Type>();

            foreach (var pair in ctx.RawTypes)
            {
                var rawType = pair.Value;

                if (rawType.DataType.Attributes != null)
                    _linkAttributes(rawType, ctx);

                if (rawType.DataType.Extends != null)
                    _linkParents(rawType, ctx);

                rawType.OntologyType.Meta = rawType.OntologyType.CreateMeta();
                types.Add(rawType.OntologyType);
            }

            return types;
        }

        private void _linkAttributes(UnSerializedType rawType, LoadingContext ctx)
        {
            var type = rawType.OntologyType;

            foreach (var attr in rawType.DataType.Attributes)
            {
                var dataType = ctx.RawTypes[attr.Id].DataType;
                var attrType = ctx.RawTypes[attr.Id].OntologyType;
                var targetType = ctx.TypesByName[attr.Target];

                attrType.AddType(targetType);
                type.AddType(attrType);
            }
        }

        private void _linkParents(UnSerializedType rawType, LoadingContext ctx)
        {
            var now = DateTime.UtcNow;

            foreach (var parentName in rawType.DataType.Extends)
            {
                var parent = ctx.TypesByName[parentName];
                var inheritance = new InheritanceRelationType(parent.Id) { CreatedAt = now, UpdatedAt = now };
                inheritance.AddType(ctx.RawTypes[parent.Id].OntologyType);
                rawType.OntologyType.AddType(inheritance);
            }
        }

        public void Invalidate()
        {
            throw new NotSupportedException("This provider does have state thus does not support invalidation");
        }

        class LoadingContext
        {
            public Dictionary<Guid, UnSerializedType> RawTypes = new Dictionary<Guid, UnSerializedType>();
            public Dictionary<string, Type> TypesByName = new Dictionary<string, Type>();
        }

        class UnSerializedType
        {
            public Type OntologyType;
            public Serializer.DataType<JObject> DataType;
        }
    }
}