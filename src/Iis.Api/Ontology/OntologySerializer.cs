using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Iis.Domain;
using Iis.Domain.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Meta;

namespace IIS.Core.Ontology
{
    public class Serializer
    {
        public void serialize(string basePath, OntologyModel ontology)
        {
            foreach (var type in ontology.Types)
            {
                if (_isVirtual(type))
                    continue;

                var dataType = _mapToDataType(type);
                _writeToFile(basePath, dataType);
            }
        }

        private bool _isVirtual(NodeType type)
        {
            return type is EmbeddingRelationType relationType && relationType.IsInversed || type is InheritanceRelationType;
        }

        private DataType<IMeta> _mapToDataType(NodeType type)
        {
            var dataType = new DataType<IMeta> {
                Id = type.Id,
                Title = type.Title,
                Name = type.Name,
                ConceptType = _logicalType(type),
                Meta = type.Meta
            };

            if (type.DirectProperties.Count() > 0)
                dataType.Attributes = _aggregateAttributes(type);

            if (type.DirectParents.Count() > 0)
                dataType.Extends = type.DirectParents.Select(t => t.Name).ToArray();

            if (type is EntityType entityType && entityType.IsAbstract)
            {
                dataType.Abstract = true;
            }
            else if (type is AttributeType attrType)
            {
                dataType.Type = attrType.ScalarTypeEnum.ToString();
            }

            return dataType;
        }

        private List<DataEntityRelation<IMeta>> _aggregateAttributes(NodeType type)
        {
            return type.DirectProperties.Aggregate(new List<DataEntityRelation<IMeta>>(), (props, prop) =>
            {
                if (prop.IsInversed)
                    return props;

                var attribute = new DataEntityRelation<IMeta> {
                    Id = prop.Id,
                    Target = prop.TargetType.Name,
                    Name = prop.Name,
                    Title = prop.Title,
                    Meta = prop.Meta
                };

                if (prop.EmbeddingOptions == EmbeddingOptions.Required)
                {
                    var meta = (RelationMetaBase)(prop.Meta ?? new RelationMetaBase());
                    meta.Validation = meta?.Validation ?? new Validation();
                    meta.Validation.Required = true;
                    prop.Meta = meta;
                }
                else if (prop.EmbeddingOptions == EmbeddingOptions.Multiple)
                {
                    var meta = (RelationMetaBase)(prop.Meta ?? new RelationMetaBase());
                    meta.Multiple = true;
                    prop.Meta = meta;
                }

                props.Add(attribute);

                return props;
            });
        }

        private string _serializeDataType(DataType<IMeta> dataType)
        {
            var settings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(dataType, Formatting.Indented, settings);
        }

        private string _logicalType(NodeType type)
        {
            if (type is EntityType)
                return "entities";
            if (type is RelationType)
                return "relations";
            if (type is AttributeType)
                return "attributes";

            throw new ArgumentException($"Cannot detect logical type of {type.Name}");
        }

        private void _writeToFile(string basePath, DataType<IMeta> dataType)
        {
            // relations currently may have the same name, so we need to append their id in file name
            var name = dataType.ConceptType == "relations" ? $"{dataType.Name}.{dataType.Id}" : dataType.Name;
            var content = _serializeDataType(dataType);

            File.WriteAllText(Path.Combine(basePath, dataType.ConceptType, $"{name}.json"), content);
        }

        public class DataType<TMeta>
        {
            public Guid Id; // we need Id because RelationType.Name is not unique but it MUST!
            public bool? Abstract;
            public string ConceptType;
            public string Name;
            public string Title;
            public string Type;
            public string[] Extends;
            public List<DataEntityRelation<TMeta>> Attributes;
            public TMeta Meta;
        }

        public class DataEntityRelation<TMeta>
        {
            public Guid Id;
            public string Name;
            public string Title;
            public TMeta Meta;
            public string Target;
        }
    }
}