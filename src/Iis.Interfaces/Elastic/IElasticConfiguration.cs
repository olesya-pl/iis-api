using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticConfiguration
    {
        void ReloadFields(IEnumerable<IElasticFieldEntity> elasticFields, string typeName = null);
        IReadOnlyList<IIisElasticField> GetOntologyIncludedFields(IEnumerable<string> typeNames);
        IReadOnlyList<IIisElasticField> GetMaterialsIncludedFields(IEnumerable<string> typeNames);
    }
}
