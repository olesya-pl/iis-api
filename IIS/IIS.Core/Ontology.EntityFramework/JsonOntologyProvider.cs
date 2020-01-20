using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;
using Iis.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology.EntityFramework
{
    public class JsonOntologyProvider : BaseOntologyProvider, IOntologyProvider
    {
        private string _basePath;

        public JsonOntologyProvider(string basePath)
        {
            _basePath = basePath;
        }

        public async Task<OntologyModel> GetOntologyAsync(CancellationToken cancellationToken = default)
        {
            var ctx = new LoadingContext();
            _loadRawTypes(ctx);
            var types = _linkTypes(ctx);

            return new OntologyModel(types);
        }

        private void _loadRawTypes(LoadingContext ctx)
        {
            var files = Directory.GetFiles(_basePath, "*.json", SearchOption.AllDirectories);

            foreach (var path in files)
            {
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
            NodeType type;

            switch (dataType.ConceptType)
            {
                case "entities":
                    type = new EntityType(dataType.Id, dataType.Name, dataType.Abstract == true);
                    break;
                case "relations":
                    type = RelationType.Build(dataType.Id, dataType.Name, _metaToEmbeddingOptions(dataType.Meta));
                    break;
                case "attributes":
                    Iis.Domain.ScalarType attrDataType;
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

            if (meta["multiple"] != null && (bool)meta["multiple"] == true)
                return EmbeddingOptions.Multiple;

            if (meta["validation"] != null && (bool)meta["validation"]["required"])
                return EmbeddingOptions.Required;

            return EmbeddingOptions.Optional;
        }

        private IEnumerable<NodeType> _linkTypes(LoadingContext ctx)
        {
            var types = new List<NodeType>();

            foreach (var pair in ctx.RawTypes)
            {
                var rawType = pair.Value;
                var type = rawType.OntologyType;

                if (rawType.DataType.Attributes != null)
                    _linkAttributes(rawType, ctx);

                if (rawType.DataType.Extends != null)
                    types.AddRange(_linkParents(rawType, ctx));

                type.Meta = type.Meta ?? type.CreateMeta();
                types.Add(type);
            }

            foreach (var pair in ctx.Relations)
            {
                var inversedRelationType = _addInversedRelation(pair.Key, pair.Value);

                if (inversedRelationType != null)
                    types.Add(inversedRelationType);
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

                // TODO: merge metadata from attr.meta with attrType.Meta
                attrType.AddType(targetType);
                type.AddType(attrType);
                ctx.Relations.Add((EmbeddingRelationType)attrType, type);
            }
        }

        private IEnumerable<InheritanceRelationType> _linkParents(UnSerializedType rawType, LoadingContext ctx)
        {
            var parentTypes = new List<InheritanceRelationType>();
            var now = DateTime.UtcNow;
            var type = rawType.OntologyType;

            foreach (var parentName in rawType.DataType.Extends)
            {
                if (_hasParent(type, parentName))
                    continue;

                var parent = ctx.TypesByName[parentName];
                var inheritance = new InheritanceRelationType(Guid.NewGuid()) { CreatedAt = now, UpdatedAt = now };
                inheritance.AddType(ctx.RawTypes[parent.Id].OntologyType);
                rawType.OntologyType.AddType(inheritance);
                parentTypes.Add(inheritance);
            }

            return parentTypes;
        }

        private bool _hasParent(NodeType type, string parentName)
        {
            return type.DirectParents.Any(parentType => parentType.Name == parentName);
        }

        public void Invalidate()
        {
            throw new NotSupportedException("This provider does have state thus does not support invalidation");
        }

        class LoadingContext
        {
            public Dictionary<Guid, UnSerializedType> RawTypes = new Dictionary<Guid, UnSerializedType>();
            public Dictionary<string, NodeType> TypesByName = new Dictionary<string, NodeType>();
            public Dictionary<EmbeddingRelationType, NodeType> Relations= new Dictionary<EmbeddingRelationType, NodeType>();
        }

        class UnSerializedType
        {
            public NodeType OntologyType;
            public Serializer.DataType<JObject> DataType;
        }
    }
}