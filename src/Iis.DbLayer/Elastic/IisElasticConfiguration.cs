using Iis.DbLayer.Repositories;
using Iis.Domain.Elastic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.DbLayer.Elastic
{
    public class IisElasticConfiguration : IElasticConfiguration
    {
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

        private IReadOnlyList<IIisElasticField> MergeConfiguredElasticFields(
            Dictionary<string, NodeAggregationInfo> ontologyFields, 
            List<IisElasticField> configuredElasticFields)
        {
            var mappedFieldsDictionary
                = ontologyFields.Select(p => new IisElasticField
                {
                    Name = p.Value.Name,
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

        private void MergeOntologyFields(
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

        private List<string> GetMaterialFieldNames()
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
                $"{nameof(MaterialDocument.Assignee)}.*"
            };
        }

        private List<NodeAggregationInfo> GetFieldsByNodeType(string typeName)
        {
            var nodeType = _ontologySchema.GetEntityTypeByName(typeName);
            if (nodeType != null)
            {
                return nodeType.GetAttributeDotNamesRecursiveWithLimit();
            }

            return new List<NodeAggregationInfo>();
        }

        public IReadOnlyList<IIisElasticField> GetMaterialsIncludedFields(IEnumerable<string> typeNames)
        {
            var fieldNames = GetMaterialFieldNames();
            var result = GetConfiguredElasticFields(typeNames);
            result.AddRange(fieldNames
                .Where(name => !result.Any(ef => ef.Name == name))
                .Select(name => new IisElasticField { Name = name })
                );
            return result;
        }
    }
}
