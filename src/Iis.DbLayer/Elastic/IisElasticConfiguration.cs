using Iis.DataModel.Cache;
using Iis.DbLayer.Repositories;
using Iis.Domain.Elastic;
using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.DbLayer.Elastic
{
    public class IisElasticConfiguration : IElasticConfiguration
    {
        IOntologySchema _ontologySchema;
        IOntologyCache _ontologyCache;
        List<IElasticFieldEntity> _elasticFields = new List<IElasticFieldEntity>();
        public IisElasticConfiguration(IOntologySchema ontologySchema, IOntologyCache ontologyCache)
        {
            _ontologySchema = ontologySchema;
            _ontologyCache = ontologyCache;
        }

        public void ReloadFields(IEnumerable<IElasticFieldEntity> elasticFields, string typeName = null)
        {
            _elasticFields.RemoveAll(ef => ef.TypeName == typeName || typeName == null);
            _elasticFields.AddRange(elasticFields);
        }

        public IReadOnlyList<IIisElasticField> GetOntologyIncludedFields(IEnumerable<string> typeNames)
        {
            var fieldNames = new List<string>();
            foreach (var typeName in typeNames)
            {
                fieldNames.AddRange(GetFieldNamesByNodeType(typeName));
            }
            if (fieldNames.Count == 0) return new List<IIisElasticField>();

            fieldNames = fieldNames.Distinct().ToList();

            var result = GetConfiguredElasticFields(typeNames);
            result.AddRange(fieldNames
                .Where(name => !result.Any(ef => ef.Name == name))
                .Select(name => new IisElasticField { Name = name })
                );
            return result.Where(p => !p.IsExcluded).ToList();
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
                $"{nameof(MaterialDocument.Data)}.*",
                $"{nameof(MaterialDocument.LoadData)}.*",
                $"{nameof(MaterialDocument.Metadata)}.*",
                $"{nameof(MaterialDocument.Transcriptions)}.*",
                $"{nameof(MaterialDocument.Children)}.*",
                $"{nameof(MaterialDocument.Assignee)}.*"
            };
        }

        private List<string> GetFieldNamesByNodeType(string typeName)
        {
            var cachedItems = _ontologyCache.GetFieldNamesByNodeType(typeName);
            if (cachedItems != null && cachedItems.Any())
            {
                return cachedItems.ToList();
            }

            var nodeType = _ontologySchema.GetEntityTypeByName(typeName);
            if (nodeType != null)
            {
                var dbItems = nodeType.GetAttributeDotNamesRecursiveWithLimit();
                _ontologyCache.PutFieldNamesByNodeType(typeName, dbItems);
                return dbItems;
            }

            return new List<string>();
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
