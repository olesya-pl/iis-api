using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Elastic;
using Iis.Elastic.Entities;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.DbLayer.Elastic
{
    public class IisElasticConfiguration : IElasticConfiguration
    {
        private static readonly IReadOnlyCollection<NodeAggregationInfo> _defaultNodeTypeFieldList = new List<NodeAggregationInfo>
        {
            new NodeAggregationInfo
            {
                Name = ElasticConfigConstants.NodeTypeNameField,
                IsAggregated = false
            },
            new NodeAggregationInfo
            {
                Name = ElasticConfigConstants.NodeTypeTitleField,
                Alias = ElasticConfigConstants.NodeTypeTitleAlias,
                IsAggregated = true
            },
            new NodeAggregationInfo
            {
                Name = ElasticConfigConstants.NodeTypeTitleAggregateField,
                IsAggregated = false
            },
            new NodeAggregationInfo
            {
                Name = ElasticConfigConstants.CreatedAtField,
                IsAggregated = false
            },
            new NodeAggregationInfo
            {
                Name = ElasticConfigConstants.UpdatedAtField,
                IsAggregated = false
            }
        };
        private readonly IOntologySchema _ontologySchema;
        private readonly List<IElasticFieldEntity> _elasticFields = new List<IElasticFieldEntity>();

        public IisElasticConfiguration(IOntologySchema ontologySchema)
        {
            _ontologySchema = ontologySchema;
        }

        public void ReloadFields(IEnumerable<IElasticFieldEntity> elasticFields, string typeName = null)
        {
            _elasticFields.RemoveAll(ef => ef.TypeName == typeName || typeName == null);
            _elasticFields.AddRange(elasticFields);
        }

        public IReadOnlyList<IIisElasticField> GetOntologyIncludedFields(IEnumerable<string> typeNames)
        {
            var fields = new Dictionary<string, NodeAggregationInfo>();
            foreach (var typeName in typeNames)
            {
                MergeOntologyFields(fields, GetFieldsByNodeType(typeName));
            }
            if (fields.Count == 0) return new List<IIisElasticField>();

            return MergeConfiguredElasticFields(fields, GetConfiguredElasticFields(typeNames));
        }

        public IReadOnlyList<IIisElasticField> GetMaterialsIncludedFields(IEnumerable<string> typeNames)
        {
            var fieldNames = GetMaterialFieldNames();
            var result = GetConfiguredElasticFields(typeNames);
            result.AddRange(fieldNames
                .Where(name => !result.Any(ef => ef.Name == name))
                .Select(name => new IisElasticField { Name = name }));
            return result;
        }

        private static IReadOnlyList<IIisElasticField> MergeConfiguredElasticFields(
            Dictionary<string, NodeAggregationInfo> ontologyFields,
            List<IisElasticField> configuredElasticFields)
        {
            var mappedFieldsDictionary
                = ontologyFields.Select(p => new IisElasticField
                {
                    Name = p.Value.Name,
                    Alias = p.Value.Alias,
                    IsAggregated = p.Value.IsAggregated
                }).ToDictionary(p => p.Name);

            foreach (var field in configuredElasticFields)
            {
                if (mappedFieldsDictionary.ContainsKey(field.Name))
                {
                    field.IsAggregated = mappedFieldsDictionary[field.Name].IsAggregated;
                    mappedFieldsDictionary[field.Name] = field;
                }
                else
                {
                    mappedFieldsDictionary.Add(field.Name, field);
                }
            }
            return mappedFieldsDictionary.Values.Where(p => !p.IsExcluded).ToList();
        }

        private static void MergeOntologyFields(
            Dictionary<string, NodeAggregationInfo> destination, IReadOnlyCollection<NodeAggregationInfo> source)
        {
            foreach (var field in source)
            {
                if (destination.ContainsKey(field.Name))
                {
                    destination[field.Name].IsAggregated = destination[field.Name].IsAggregated || field.IsAggregated;
                }
                else
                {
                    destination.Add(field.Name, field);
                }
            }
        }

        private static List<string> GetMaterialFieldNames()
        {
            return new List<string>()
            {
                nameof(MaterialDocument.Id),
                nameof(MaterialDocument.FileId),
                nameof(MaterialDocument.ParentId),
                nameof(MaterialDocument.Source),
                nameof(MaterialDocument.Type),
                nameof(MaterialDocument.Content),
                nameof(MaterialDocument.CreatedDate),
                nameof(MaterialDocument.UpdatedAt),
                nameof(MaterialDocument.Importance),
                nameof(MaterialDocument.Reliability),
                nameof(MaterialDocument.Relevance),
                nameof(MaterialDocument.Completeness),
                nameof(MaterialDocument.SourceReliability),
                nameof(MaterialDocument.Title),
                nameof(MaterialDocument.ProcessedStatus),
                nameof(MaterialDocument.SessionPriority),
                nameof(MaterialDocument.MlHandlersCount),
                nameof(MaterialDocument.ProcessedMlHandlersCount),
                $"{nameof(MaterialDocument.MLResponses)}.*",
                $"{nameof(MaterialDocument.LoadData)}.*",
                $"{nameof(MaterialDocument.Metadata)}.*",
                $"{nameof(MaterialDocument.Transcriptions)}.*",
                $"{nameof(MaterialDocument.Children)}.*",
                $"{nameof(MaterialDocument.Assignees)}.*",
                $"{nameof(MaterialDocument.Editor)}.*"
            };
        }

        private List<IisElasticField> GetConfiguredElasticFields(IEnumerable<string> typeNames)
        {
            var elasticFields = _elasticFields
                            .Where(ef => typeNames.Contains(ef.TypeName))
                            .ToList();
            var result = elasticFields.Select(ef => new IisElasticField
                                                    {
                                                        Name = ef.Name,
                                                        IsExcluded = ef.IsExcluded,
                                                        Fuzziness = ef.Fuzziness,
                                                        Boost = ef.Boost
                                                    })
                .GroupBy(p => p.Name)
                .Select(group => group.First())
                .ToList();
            return result;
        }

        private List<NodeAggregationInfo> GetFieldsByNodeType(string typeName)
        {
            var nodeType = _ontologySchema.GetEntityTypeByName(typeName);
            if (nodeType != null)
            {
                var nodes = _ontologySchema.GetAttributesInfo(typeName)
                    .Items
                    .Select(p => new NodeAggregationInfo
                    {
                        Name = p.DotName,
                        Alias = p.AliasesList?.FirstOrDefault(),
                        IsAggregated = p.IsAggregated
                    }).ToList();

                nodes.AddRange(_defaultNodeTypeFieldList);

                return nodes;
            }

            return new List<NodeAggregationInfo>();
        }
    }
}
