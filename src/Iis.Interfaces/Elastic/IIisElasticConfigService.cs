using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Interfaces.Elastic
{
    public interface IIisElasticConfigService
    {
        Task<IReadOnlyList<IElasticFieldEntity>> SaveElasticFieldsAsync(string typeName, IReadOnlyList<IIisElasticField> fields);
    }
}