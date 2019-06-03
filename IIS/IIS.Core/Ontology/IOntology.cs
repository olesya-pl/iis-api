using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IOntology
    {
        Task<IEnumerable<Entity>> GetEntitiesAsync(string typeName);

        //Task<IDictionary<(long, string), IOntologyNode>> GetEntitiesByAsync(IEnumerable<(long, string)> entityIds);

        //Task<IDictionary<string, ArrayRelation>> GetEntitiesAsync(IEnumerable<string> typeNames);
    }
}
