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
        IOntologySchema _ontologySchema;
        List<IElasticFieldEntity> _elasticFields = new List<IElasticFieldEntity>();
        public IisElasticConfiguration(IOntologySchema ontologySchema)
        {
            _ontologySchema = ontologySchema;
        }

        public void ReloadFields(IEnumerable<IElasticFieldEntity> elasticFields, string typeName = null)
        {
            _elasticFields.RemoveAll(ef => ef.TypeName == typeName || typeName == null);
            _elasticFields.AddRange(elasticFields);
        }

        public IReadOnlyList<IIisElasticField> GetIncludedFieldsByTypeNames(IEnumerable<string> typeNames)
        {
            var fieldNames = new List<string>();
            foreach (var typeName in typeNames)
            {
                if (typeName == "Person" || typeName == "MilitaryOrganization" || typeName == "Subdivision")
                    continue;

                fieldNames.AddRange(GetFieldNamesByNodeType(typeName));
            }
            if (fieldNames.Count == 0) return new List<IIisElasticField>();

            fieldNames = fieldNames.Distinct().ToList();

            var elasticFields = _elasticFields
                .Where(ef => typeNames.Contains(ef.TypeName) && !ef.IsExcluded)
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
            result.AddRange(fieldNames
                .Where(name => !elasticFields.Any(ef => ef.Name == name))
                .Select(name => new IisElasticField { Name = name })
                );
            return result;
        }

        private List<string> GetFieldNamesByNodeType(string typeName)
        {
            var nodeType = _ontologySchema.GetEntityTypeByName(typeName);
            if (nodeType != null)
            {
                return nodeType.GetAttributeDotNamesRecursive();
            }

            return new List<string>();
        }
    }
}
